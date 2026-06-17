namespace AgileConfig.Server.SyncPlugin.Models;

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
