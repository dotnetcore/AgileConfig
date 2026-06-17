using AgileConfig.Server.SyncPlugin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.SyncPlugin;

/// <summary>
/// Extension methods for registering SyncPlugin services and plugins
/// </summary>
public static class SyncPluginExtensions
{
    /// <summary>
    /// Add SyncPlugin services to the service collection
    /// </summary>
    public static IServiceCollection AddSyncPlugin(this IServiceCollection services)
    {
        services.AddSingleton<SyncEngine>();
        services.AddSingleton<Retry.SyncRetryService>();
        services.AddHostedService<BackgroundServices.SyncRetryBackgroundService>();
        services.AddHostedService<BackgroundServices.SyncPluginInitializer>();

        return services;
    }

    /// <summary>
    /// Register built-in sync plugins
    /// Should be called after AddSyncPlugin
    /// </summary>
    public static IServiceCollection AddSyncPluginBuiltIn(this IServiceCollection services, ILoggerFactory loggerFactory)
    {
        // This will be called during service provider building
        // The plugins will be registered in SyncPluginInitializer
        
        return services;
    }
}
