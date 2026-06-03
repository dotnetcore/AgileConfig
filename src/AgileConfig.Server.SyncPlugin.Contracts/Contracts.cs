namespace AgileConfig.Server.SyncPlugin.Contracts;

/// <summary>
/// Interface for sync plugins
/// All sync operations use "replace all" strategy: delete all + insert all
/// </summary>
public interface ISyncPlugin
{
    /// <summary>
    /// Unique name of the plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Display name for UI
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Initialize the plugin with configuration
    /// </summary>
    Task<SyncPluginResult> InitializeAsync(SyncPluginConfig config);

    /// <summary>
    /// Full sync: delete all + insert all for the given app+env
    /// This is the ONLY sync method - no need to handle add/update/delete separately
    /// </summary>
    /// <param name="contexts">All current published configs for the app+env</param>
    Task<SyncPluginResult> SyncAllAsync(SyncContext[] contexts);

    /// <summary>
    /// Health check for the plugin
    /// </summary>
    Task<SyncPluginHealthResult> HealthCheckAsync();

    /// <summary>
    /// Shutdown the plugin
    /// </summary>
    Task ShutdownAsync();
}

/// <summary>
/// Result of sync plugin operation
/// </summary>
public class SyncPluginResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
}

/// <summary>
/// Health check result of sync plugin
/// </summary>
public class SyncPluginHealthResult
{
    public bool Healthy { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Configuration for sync plugin
/// </summary>
public class SyncPluginConfig
{
    public string? PluginName { get; set; }
    public string? Enabled { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new();
}

/// <summary>
/// Context for sync operation
/// </summary>
public class SyncContext
{
    /// <summary>
    /// Application Id
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Application Name
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Environment (e.g., PROD, DEV)
    /// </summary>
    public string Env { get; set; } = string.Empty;

    /// <summary>
    /// Config key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Config value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Config group
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Operation type: Add, Update, Delete
    /// </summary>
    public SyncOperationType OperationType { get; set; }

    /// <summary>
    /// Timestamp of the change
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

public enum SyncOperationType
{
    Add,
    Update,
    Delete
}
