using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Data.Abstraction;

public sealed class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(TimeSpan retryAfter, Exception innerException = null)
        : base("The database is temporarily unavailable. Please retry later.", innerException)
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan RetryAfter { get; }
}

public sealed class DatabaseCircuitBreaker(
    TimeProvider timeProvider,
    ILogger<DatabaseCircuitBreaker> logger,
    Func<Exception, bool> isUnavailable,
    int failureThreshold,
    TimeSpan breakDuration)
{
    private readonly object _gate = new();
    private int _failures;
    private long _generation;
    private long _failureVersion;
    private DateTimeOffset? _retryAt;
    private bool _probing;

    public long FailureVersion
    {
        get { lock (_gate) return _failureVersion; }
    }

    public void ThrowIfOpen()
    {
        lock (_gate)
        {
            if (_retryAt.HasValue && (_probing || _retryAt.Value > timeProvider.GetUtcNow()))
                throw new DatabaseUnavailableException(breakDuration);
        }
    }

    public T CreateResource<T>(Func<T> create)
    {
        long generation;
        lock (_gate)
        {
            ThrowIfOpen();
            generation = _generation;
        }

        try
        {
            // Creating a cached provider or assigning a transaction does not prove connectivity.
            return create();
        }
        catch (Exception exception)
        {
            Fail(generation, exception);
            throw;
        }
    }

    public T Execute<T>(Func<T> operation)
    {
        var generation = Enter();
        try
        {
            var result = operation();
            Succeed(generation);
            return result;
        }
        catch (Exception exception)
        {
            Fail(generation, exception);
            throw;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var generation = Enter();
        try
        {
            var result = await operation();
            Succeed(generation);
            return result;
        }
        catch (Exception exception)
        {
            Fail(generation, exception);
            throw;
        }
    }

    private long Enter()
    {
        lock (_gate)
        {
            if (_retryAt.HasValue)
            {
                var remaining = _retryAt.Value - timeProvider.GetUtcNow();
                if (_probing || remaining > TimeSpan.Zero)
                    throw new DatabaseUnavailableException(remaining > TimeSpan.Zero ? remaining : breakDuration);

                _probing = true;
            }

            return _generation;
        }
    }

    private void Succeed(long generation)
    {
        lock (_gate)
        {
            // A request admitted before the circuit opened must not close it.
            if (generation != _generation) return;
            if (_retryAt.HasValue)
            {
                logger.LogInformation("Database circuit recovered.");
                _generation++;
            }

            _failures = 0;
            _retryAt = null;
            _probing = false;
        }
    }

    private void Fail(long generation, Exception exception)
    {
        var unavailable = isUnavailable(exception);
        lock (_gate)
        {
            if (unavailable) _failureVersion++;
            if (generation == _generation)
            {
                if (unavailable && (++_failures >= failureThreshold || _probing))
                {
                    _generation++;
                    _retryAt = timeProvider.GetUtcNow() + breakDuration;
                    _probing = false;
                    logger.LogWarning(exception, "Database circuit opened for {Seconds} seconds after a connectivity failure.",
                        breakDuration.TotalSeconds);
                }
                else if (_probing)
                {
                    // A failed probe is not evidence of recovery, even for an unrelated error.
                    _probing = false;
                    _retryAt = timeProvider.GetUtcNow() + breakDuration;
                }
                else if (unavailable && _failures == 1)
                {
                    logger.LogError(exception, "Database connectivity failed.");
                }
            }
        }

        if (unavailable)
            throw new DatabaseUnavailableException(breakDuration, exception);
    }
}
