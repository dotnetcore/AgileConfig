using System;
using AgileConfig.Server.Data.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public sealed class ServiceHealthCheckRecovery(
    DatabaseCircuitBreaker circuit,
    TimeProvider timeProvider,
    IConfiguration configuration,
    ILogger<ServiceHealthCheckRecovery> logger)
{
    private long _failureVersion;
    private bool _needsRecovery = true;
    private DateTimeOffset _graceUntil;

    public void RequireRecovery()
    {
        _needsRecovery = true;
    }

    // Called by the single health worker only after a successful database read.
    public bool IsInGracePeriod()
    {
        var failureVersion = circuit.FailureVersion;
        if (_needsRecovery || failureVersion != _failureVersion)
        {
            var seconds = configuration.GetValue("serviceHealthCheckRecoveryGraceSeconds", 60);
            if (seconds <= 0)
                throw new ArgumentException("serviceHealthCheckRecoveryGraceSeconds must be greater than 0.");

            _graceUntil = timeProvider.GetUtcNow().AddSeconds(seconds);
            _failureVersion = failureVersion;
            _needsRecovery = false;
            logger.LogWarning("Pausing heartbeat expiry for {Seconds} seconds after startup or database recovery.",
                seconds);
        }

        return timeProvider.GetUtcNow() < _graceUntil;
    }
}
