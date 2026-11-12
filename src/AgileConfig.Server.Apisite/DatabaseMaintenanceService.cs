using System;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite;

public sealed class InitializationState
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task Ready => _ready.Task;
    public bool IsReady => _ready.Task.IsCompletedSuccessfully;
    public void MarkReady() => _ready.TrySetResult();
}

public abstract class DatabaseMaintenanceService(
    IServiceScopeFactory scopeFactory,
    InitializationState initialization,
    TimeProvider timeProvider,
    ILogger logger) : BackgroundService
{
    protected abstract TimeSpan Interval { get; }
    protected abstract Task RunOnceAsync(IServiceProvider services, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await initialization.Ready.WaitAsync(stoppingToken);
        var failures = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = Interval;
            try
            {
                using var scope = scopeFactory.CreateScope();
                await RunOnceAsync(scope.ServiceProvider, stoppingToken);
                if (failures > 0) logger.LogInformation("Database maintenance resumed.");
                failures = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                delay = RetryDelay(++failures);
                logger.LogError(exception, "Database maintenance failed; retrying in {Seconds} seconds.",
                    delay.TotalSeconds);
            }

            await Task.Delay(delay, timeProvider, stoppingToken);
        }
    }

    internal static TimeSpan RetryDelay(int failures) =>
        TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, Math.Min(failures, 5))));
}

public sealed class NodeMonitorService(
    IServiceScopeFactory scopes, InitializationState initialization, TimeProvider time,
    ILogger<NodeMonitorService> logger)
    : DatabaseMaintenanceService(scopes, initialization, time, logger)
{
    protected override TimeSpan Interval => TimeSpan.FromSeconds(5);

    protected override Task RunOnceAsync(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<IRemoteServerNodeProxy>().TestEchoAsync(cancellationToken);
}

public sealed class RegistryMonitorService(
    IServiceScopeFactory scopes, InitializationState initialization, TimeProvider time,
    ILogger<RegistryMonitorService> logger, IConfiguration configuration)
    : DatabaseMaintenanceService(scopes, initialization, time, logger)
{
    protected override TimeSpan Interval
    {
        get
        {
            var seconds = configuration.GetValue("serviceHealthCheckInterval", 15);
            if (seconds <= 0) throw new ArgumentException("serviceHealthCheckInterval must be greater than 0.");
            return TimeSpan.FromSeconds(seconds);
        }
    }

    protected override Task RunOnceAsync(IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<IServiceHealthCheckService>().CheckAsync(cancellationToken);
}
