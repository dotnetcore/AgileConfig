using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using AgileConfig.Server.SyncPlugin;
using AgileConfig.Server.SyncPlugin.Contracts;
using AgileConfig.Server.SyncPlugin.Retry;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.EventHandler;

/// <summary>
/// Event handler that syncs published configs to external systems via SyncPlugin
/// Uses "replace all" strategy - always fetches latest configs and replaces all
/// </summary>
public class ConfigSyncEventHandler : IEventHandler<PublishConfigSuccessful>
{
    private readonly IConfigService _configService;
    private readonly SyncEngine _syncEngine;
    private readonly SyncRetryService _retryService;
    private readonly Microsoft.Extensions.Logging.ILogger<ConfigSyncEventHandler> _logger;

    public ConfigSyncEventHandler(
        IConfigService configService,
        SyncEngine syncEngine,
        SyncRetryService retryService,
        Microsoft.Extensions.Logging.ILogger<ConfigSyncEventHandler> logger)
    {
        _configService = configService;
        _syncEngine = syncEngine;
        _retryService = retryService;
        _logger = logger;
    }

    public async Task Handle(IEvent evt)
    {
        var evtInstance = evt as PublishConfigSuccessful;
        var timeline = evtInstance.PublishTimeline;
        
        if (timeline == null)
        {
            _logger.LogWarning("PublishConfigSuccessful event has no timeline");
            return;
        }

        try
        {
            // Get all published configs for this app and env
            var configs = await _configService.GetPublishedConfigsAsync(timeline.AppId, timeline.Env);
            
            if (configs == null || !configs.Any())
            {
                _logger.LogInformation("No published configs found for app {AppId} env {Env}", timeline.AppId, timeline.Env);
                return;
            }

            // Clear existing failed records for this app+env before new sync
            _retryService.ClearFailedRecord(timeline.AppId, timeline.Env);
            
            // Convert to sync contexts
            var contexts = configs.Select(c => new SyncContext
            {
                AppId = c.AppId,
                AppName = timeline.AppId,
                Env = c.Env,
                Key = c.Key,
                Value = c.Value ?? "",
                Group = c.Group,
                OperationType = SyncOperationType.Add,
                Timestamp = DateTimeOffset.UtcNow
            }).ToArray();

            // Full sync using "replace all" strategy
            var result = await _syncEngine.SyncAllAsync(contexts);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully synced {Count} configs for app {AppId} env {Env}", 
                    contexts.Length, timeline.AppId, timeline.Env);
            }
            else
            {
                _logger.LogWarning("Failed to sync configs for app {AppId} env {Env}: {Message}", 
                    timeline.AppId, timeline.Env, result.Message);
                
                // Record for retry
                _retryService.RecordFailed(timeline.AppId, timeline.Env, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during config sync for app {AppId} env {Env}", 
                timeline.AppId, timeline.Env);
            
            // Record for retry
            _retryService.RecordFailed(timeline.AppId, timeline.Env, ex.Message);
        }
    }
}
