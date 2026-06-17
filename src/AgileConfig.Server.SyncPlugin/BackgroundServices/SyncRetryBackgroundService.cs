using AgileConfig.Server.SyncPlugin.Retry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.SyncPlugin.BackgroundServices;

/// <summary>
/// Background service that periodically processes failed sync records
/// </summary>
public class SyncRetryBackgroundService : BackgroundService
{
    private readonly SyncRetryService _retryService;
    private readonly ILogger<SyncRetryBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public SyncRetryBackgroundService(
        SyncRetryService retryService,
        ILogger<SyncRetryBackgroundService> logger)
    {
        _retryService = retryService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncRetryBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _retryService.ProcessFailedRecordsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing failed sync records");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("SyncRetryBackgroundService stopped");
    }
}
