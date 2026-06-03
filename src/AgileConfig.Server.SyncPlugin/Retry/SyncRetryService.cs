using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using AgileConfig.Server.SyncPlugin.Contracts;

namespace AgileConfig.Server.SyncPlugin.Retry;

/// <summary>
/// Service that handles sync retry logic
/// Uses "replace all" strategy - always fetches latest configs from DB on retry
/// </summary>
public class SyncRetryService
{
    private readonly SyncEngine _syncEngine;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncRetryService> _logger;
    private readonly List<FailedSyncRecord> _failedRecords = new();
    private readonly object _lock = new();

    // Configuration
    private const int MaxRetryCount = 10;
    private const int CircuitBreakDurationMinutes = 60;
    private const int MinRetryDelaySeconds = 1;
    private const int MaxRetryDelaySeconds = 60;

    public SyncRetryService(
        SyncEngine syncEngine,
        IServiceProvider serviceProvider,
        ILogger<SyncRetryService> logger)
    {
        _syncEngine = syncEngine;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Record a failed sync attempt
    /// </summary>
    public void RecordFailed(string appId, string env, string? errorMessage = null)
    {
        lock (_lock)
        {
            // Check if already exists
            var existing = _failedRecords.FirstOrDefault(x => x.AppId == appId && x.Env == env);
            
            if (existing != null)
            {
                existing.RetryCount++;
                existing.LastRetryTime = DateTimeOffset.UtcNow;
                existing.LastError = errorMessage;

                if (existing.RetryCount >= MaxRetryCount)
                {
                    // Circuit break: stop retrying for CircuitBreakDurationMinutes
                    existing.IsCircuitBroken = true;
                    existing.NextRetryTime = DateTimeOffset.UtcNow.AddMinutes(CircuitBreakDurationMinutes);
                    _logger.LogError("Sync failed for app {AppId} env {Env} for {Count} times, circuit breaker activated, next retry after {NextRetry}", 
                        appId, env, existing.RetryCount, existing.NextRetryTime);
                }
                else
                {
                    // Exponential backoff: 2^retryCount seconds, capped at MaxRetryDelaySeconds
                    var delaySeconds = Math.Min(MaxRetryDelaySeconds, Math.Pow(2, existing.RetryCount));
                    existing.NextRetryTime = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
                    _logger.LogWarning("Sync failed for app {AppId} env {Env}, retry count: {Count}, next retry after {NextRetry} ({Delay}s)", 
                        appId, env, existing.RetryCount, existing.NextRetryTime, delaySeconds);
                }
            }
            else
            {
                var newRecord = new FailedSyncRecord
                {
                    AppId = appId,
                    Env = env,
                    FailedTime = DateTimeOffset.UtcNow,
                    RetryCount = 1,
                    LastError = errorMessage,
                    NextRetryTime = DateTimeOffset.UtcNow.AddSeconds(MinRetryDelaySeconds)
                };
                _failedRecords.Add(newRecord);
                _logger.LogWarning("Recorded failed sync for app {AppId} env {Env}, first retry after {NextRetry} ({Delay}s)", 
                    appId, env, newRecord.NextRetryTime, MinRetryDelaySeconds);
            }
        }
    }

    /// <summary>
    /// Process all failed records - retry sync
    /// This should be called periodically by a background service
    /// </summary>
    public async Task ProcessFailedRecordsAsync()
    {
        List<FailedSyncRecord> recordsToProcess;
        
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;
            // Get records that are ready to retry
            recordsToProcess = _failedRecords
                .Where(x => x.NextRetryTime <= now)
                .ToList();
            
            // Reset circuit breaker for records where next retry time has passed
            foreach (var record in recordsToProcess.Where(x => x.IsCircuitBroken))
            {
                record.IsCircuitBroken = false;
                record.RetryCount = 0; // Reset retry count after circuit break
                _logger.LogInformation("Circuit breaker reset for app {AppId} env {Env}, resuming retries", record.AppId, record.Env);
            }
        }

        if (!recordsToProcess.Any())
        {
            _logger.LogDebug("No failed sync records to process");
            return;
        }

        _logger.LogInformation("Processing {Count} failed sync records", recordsToProcess.Count);

        foreach (var record in recordsToProcess)
        {
            await RetrySyncAsync(record);
        }
    }

    /// <summary>
    /// Retry sync for a specific record
    /// Uses "replace all" strategy - fetches latest configs from DB
    /// </summary>
    private async Task RetrySyncAsync(FailedSyncRecord record)
    {
        // Create a scope to resolve scoped services
        using var scope = _serviceProvider.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IConfigService>();
        
        try
        {
            _logger.LogInformation("Retrying sync for app {AppId} env {Env}, attempt {Attempt}", 
                record.AppId, record.Env, record.RetryCount);

            // Get latest configs from database
            var configs = await configService.GetPublishedConfigsAsync(record.AppId, record.Env);
            
            if (configs == null || !configs.Any())
            {
                _logger.LogInformation("No configs found for app {AppId} env {Env}, removing failed record", 
                    record.AppId, record.Env);
                
                lock (_lock)
                {
                    _failedRecords.RemoveAll(x => x.AppId == record.AppId && x.Env == record.Env);
                }
                return;
            }

            // Convert to sync contexts
            var contexts = configs.Select(c => new SyncContext
            {
                AppId = c.AppId,
                AppName = c.AppId, // Use AppId as AppName
                Env = c.Env,
                Key = c.Key,
                Value = c.Value ?? "",
                Group = c.Group,
                OperationType = SyncOperationType.Add,
                Timestamp = DateTimeOffset.UtcNow
            }).ToArray();

            // Full sync
            var result = await _syncEngine.SyncAllAsync(contexts);

            if (result.Success)
            {
                _logger.LogInformation("Retry successful for app {AppId} env {Env}", record.AppId, record.Env);
                
                lock (_lock)
                {
                    _failedRecords.RemoveAll(x => x.AppId == record.AppId && x.Env == record.Env);
                }
            }
            else
            {
                _logger.LogWarning("Retry failed for app {AppId} env {Env}: {Error}", 
                    record.AppId, record.Env, result.Message);
                
                lock (_lock)
                {
                    var rec = _failedRecords.FirstOrDefault(x => x.AppId == record.AppId && x.Env == record.Env);
                    if (rec != null)
                    {
                        rec.LastError = result.Message;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during retry for app {AppId} env {Env}", record.AppId, record.Env);
        }
    }

    /// <summary>
    /// Get all failed records
    /// </summary>
    public List<FailedSyncRecord> GetFailedRecords()
    {
        lock (_lock)
        {
            return _failedRecords.ToList();
        }
    }

    /// <summary>
    /// Clear all failed records (for testing)
    /// </summary>
    public void ClearFailedRecords()
    {
        lock (_lock)
        {
            _failedRecords.Clear();
        }
    }

    /// <summary>
    /// Clear failed records for specific appId and env
    /// </summary>
    public void ClearFailedRecord(string appId, string env)
    {
        lock (_lock)
        {
            var removed = _failedRecords.RemoveAll(x => x.AppId == appId && x.Env == env);
            if (removed > 0)
            {
                _logger.LogDebug("Cleared {Count} failed records for app {AppId} env {Env}", removed, appId, env);
            }
        }
    }
}
