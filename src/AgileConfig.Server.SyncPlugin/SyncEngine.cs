using Microsoft.Extensions.Logging;
using AgileConfig.Server.SyncPlugin.Contracts;

namespace AgileConfig.Server.SyncPlugin;

/// <summary>
/// Sync engine that manages all plugins and handles config synchronization
/// Uses "replace all" strategy: delete all + insert all for each sync
/// </summary>
public class SyncEngine : IDisposable
{
    private readonly ILogger<SyncEngine> _logger;
    private readonly Dictionary<string, ISyncPlugin> _plugins = new();
    private readonly Dictionary<string, SyncPluginConfig> _pluginConfigs = new();
    private bool _initialized = false;
    private readonly object _lock = new();

    public SyncEngine(ILogger<SyncEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register a sync plugin
    /// </summary>
    public void RegisterPlugin(ISyncPlugin plugin, SyncPluginConfig config)
    {
        lock (_lock)
        {
            if (_plugins.ContainsKey(plugin.Name))
            {
                _logger.LogWarning("Plugin {PluginName} already registered, skipping", plugin.Name);
                return;
            }

            _plugins[plugin.Name] = plugin;
            _pluginConfigs[plugin.Name] = config;
            _logger.LogInformation("Registered sync plugin: {PluginName}", plugin.Name);
        }
    }

    /// <summary>
    /// Initialize all registered plugins
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;
        }

        foreach (var kvp in _plugins)
        {
            var pluginName = kvp.Key;
            var plugin = kvp.Value;
            var config = _pluginConfigs[pluginName];

            try
            {
                var result = await plugin.InitializeAsync(config);
                if (result.Success)
                {
                    _logger.LogInformation("Initialized sync plugin: {PluginName}", pluginName);
                }
                else
                {
                    _logger.LogError("Failed to initialize plugin {PluginName}: {Message}", pluginName, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception initializing plugin {PluginName}", pluginName);
            }
        }
    }

    /// <summary>
    /// Full sync to all enabled plugins using "replace all" strategy
    /// </summary>
    public async Task<SyncPluginResult> SyncAllAsync(SyncContext[] contexts)
    {
        var enabledPlugins = _plugins.Values
            .Where(p => IsPluginEnabled(p.Name))
            .ToList();

        if (!enabledPlugins.Any())
        {
            _logger.LogDebug("No enabled plugins, skipping sync");
            return new SyncPluginResult { Success = true, Message = "No enabled plugins" };
        }

        var tasks = enabledPlugins
            .Select(p => SafeExecuteAsync(() => p.SyncAllAsync(contexts)));

        var results = await Task.WhenAll(tasks);
        
        var failed = results.Where(r => !r.Success).ToList();
        if (failed.Any())
        {
            var failedPlugins = string.Join(", ", failed.Select(f => f.Message));
            _logger.LogWarning("Sync failed for {Count} plugins: {FailedPlugins}", failed.Count, failedPlugins);
            
            return new SyncPluginResult
            {
                Success = false,
                Message = $"Sync failed for {failed.Count} plugins: {failedPlugins}"
            };
        }

        _logger.LogInformation("Successfully synced {Count} configs to {PluginCount} plugins", 
            contexts.Length, enabledPlugins.Count);
        
        return new SyncPluginResult { Success = true, Message = $"Synced to {enabledPlugins.Count} plugins" };
    }

    /// <summary>
    /// Check health of all plugins
    /// </summary>
    public async Task<Dictionary<string, SyncPluginHealthResult>> HealthCheckAsync()
    {
        var tasks = _plugins.Values
            .Where(p => IsPluginEnabled(p.Name))
            .Select(async p =>
            {
                try
                {
                    var result = await p.HealthCheckAsync();
                    return (Name: p.Name, Result: result);
                }
                catch (Exception ex)
                {
                    return (Name: p.Name, Result: new SyncPluginHealthResult
                    {
                        Healthy = false,
                        Message = ex.Message
                    });
                }
            });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(r => r.Name, r => r.Result);
    }

    /// <summary>
    /// Get all registered plugins
    /// </summary>
    public IReadOnlyDictionary<string, ISyncPlugin> GetPlugins() => _plugins;

    /// <summary>
    /// Get plugin by name
    /// </summary>
    public ISyncPlugin? GetPlugin(string name) => _plugins.GetValueOrDefault(name);

    /// <summary>
    /// Get list of enabled plugin names
    /// </summary>
    public List<string> GetEnabledPluginNames()
    {
        return _plugins.Keys.Where(IsPluginEnabled).ToList();
    }

    private bool IsPluginEnabled(string pluginName)
    {
        if (!_pluginConfigs.TryGetValue(pluginName, out var config))
            return false;

        var raw = config.Enabled?.Trim();
        if (string.IsNullOrEmpty(raw))
            return false;

        if (bool.TryParse(raw, out var parsed))
            return parsed;

        return raw.Equals("1", StringComparison.OrdinalIgnoreCase)
               || raw.Equals("yes", StringComparison.OrdinalIgnoreCase)
               || raw.Equals("on", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<SyncPluginResult> SafeExecuteAsync(Func<Task<SyncPluginResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during sync operation");
            return new SyncPluginResult
            {
                Success = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public async Task ShutdownAsync()
    {
        foreach (var plugin in _plugins.Values)
        {
            try
            {
                await plugin.ShutdownAsync();
                _logger.LogInformation("Shutdown plugin: {PluginName}", plugin.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down plugin: {PluginName}", plugin.Name);
            }
        }
    }

    public void Dispose()
    {
        ShutdownAsync().GetAwaiter().GetResult();
    }
}
