using Microsoft.Extensions.Logging;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.SyncPlugin.Contracts;

namespace AgileConfig.Server.SyncPlugin;

/// <summary>
/// Service that handles config sync operations
/// Now simplified - sync is handled by event handlers, this service is for manual sync if needed
/// </summary>
public class ConfigSyncService
{
    private readonly SyncEngine _syncEngine;
    private readonly ILogger<ConfigSyncService> _logger;

    public ConfigSyncService(SyncEngine syncEngine, ILogger<ConfigSyncService> logger)
    {
        _syncEngine = syncEngine;
        _logger = logger;
    }

    /// <summary>
    /// Full sync all configs for an app+env
    /// Uses "replace all" strategy
    /// </summary>
    public async Task<bool> SyncAllAsync(Config[] configs, string env)
    {
        if (configs == null || !configs.Any())
        {
            _logger.LogInformation("No configs to sync");
            return true;
        }

        var contexts = configs.Select(c => new SyncContext
        {
            AppId = c.AppId,
            AppName = c.AppId,
            Env = env,
            Key = c.Key,
            Value = c.Value ?? "",
            Group = c.Group,
            OperationType = SyncOperationType.Add,
            Timestamp = DateTimeOffset.UtcNow
        }).ToArray();

        try
        {
            var result = await _syncEngine.SyncAllAsync(contexts);
            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during full sync");
            return false;
        }
    }

    /// <summary>
    /// Health check all sync plugins
    /// </summary>
    public async Task<Dictionary<string, SyncPluginHealthResult>> HealthCheckAsync()
    {
        return await _syncEngine.HealthCheckAsync();
    }
}
