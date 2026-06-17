namespace AgileConfig.Server.SyncPlugin.Retry;

/// <summary>
/// Record of failed sync attempts
/// Only stores (appId, env) - not the actual config values
/// When retrying, we always fetch the latest configs from database
/// </summary>
public class FailedSyncRecord
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Application ID
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Environment (e.g., PROD, DEV)
    /// </summary>
    public string Env { get; set; } = string.Empty;

    /// <summary>
    /// When the sync first failed
    /// </summary>
    public DateTimeOffset FailedTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Last retry time
    /// </summary>
    public DateTimeOffset? LastRetryTime { get; set; }

    /// <summary>
    /// Last error message
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Next retry time
    /// </summary>
    public DateTimeOffset? NextRetryTime { get; set; }

    /// <summary>
    /// Whether the record is in circuit breaker state
    /// </summary>
    public bool IsCircuitBroken { get; set; }
}
