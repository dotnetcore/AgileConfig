using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AgileConfig.Server.SyncPlugin.Plugins.Etcd;
using AgileConfig.Server.SyncPlugin.Contracts;

namespace AgileConfig.Server.SyncPlugin.BackgroundServices;

/// <summary>
/// Configuration model for SyncPlugin section in appsettings.json
/// </summary>
public class SyncPluginConfiguration
{
    public bool Enabled { get; set; } = true;
    public Dictionary<string, PluginConfig> Plugins { get; set; } = new();
}

public class PluginConfig
{
    public string Enabled { get; set; } = "false";
    public Dictionary<string, string> Settings { get; set; } = new();
}

/// <summary>
/// Initializes SyncEngine and registers plugins on application startup
/// </summary>
public class SyncPluginInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SyncEngine _syncEngine;
    private readonly ILogger<SyncPluginInitializer> _logger;
    private readonly IConfiguration _configuration;

    public SyncPluginInitializer(
        IServiceProvider serviceProvider,
        SyncEngine syncEngine,
        ILogger<SyncPluginInitializer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _syncEngine = syncEngine;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing SyncPlugin...");

        try
        {
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

            // Read configuration from appsettings.json
            var syncPluginConfig = _configuration.GetSection("SyncPlugin").Get<SyncPluginConfiguration>();
            
            if (syncPluginConfig == null || !syncPluginConfig.Enabled)
            {
                _logger.LogInformation("SyncPlugin is disabled in configuration");
                return;
            }

            // Register built-in plugins from configuration
            RegisterBuiltInPlugins(_syncEngine, loggerFactory, syncPluginConfig);

            // Initialize all registered plugins
            await _syncEngine.InitializeAsync();

            _logger.LogInformation("SyncPlugin initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SyncPlugin");
        }
    }

    private void RegisterBuiltInPlugins(SyncEngine syncEngine, ILoggerFactory loggerFactory, SyncPluginConfiguration config)
    {
        // Register Etcd plugin from configuration
        if (config.Plugins.TryGetValue("etcd", out var etcdConfig))
        {
            try
            {
                var etcdLogger = loggerFactory.CreateLogger<EtcdSyncPlugin>();
                var etcdPlugin = new EtcdSyncPlugin(etcdLogger);
                
                syncEngine.RegisterPlugin(etcdPlugin, new SyncPluginConfig
                {
                    PluginName = "etcd",
                    Enabled = etcdConfig.Enabled,
                    Settings = etcdConfig.Settings
                });
                
                _logger.LogInformation("Registered Etcd sync plugin with endpoints: {Endpoints}", 
                    etcdConfig.Settings.GetValueOrDefault("endpoints", ""));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to register Etcd plugin");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _syncEngine.ShutdownAsync();
    }
}
