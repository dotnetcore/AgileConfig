using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using AgileConfig.Server.SyncPlugin.Contracts;

namespace AgileConfig.Server.SyncPlugin.Plugins.Etcd;

/// <summary>
/// Etcd sync plugin implementation using HTTP API
/// Uses "replace all" strategy: delete all keys for app+env, then insert all
/// </summary>
public class EtcdSyncPlugin : ISyncPlugin
{
    private readonly ILogger<EtcdSyncPlugin> _logger;
    private SyncPluginConfig? _config;
    private HttpClient? _httpClient;
    private string _keyPrefix = "/agileconfig";

    public string Name => "etcd";
    public string DisplayName => "Etcd";
    public string Description => "Sync configs to etcd using replace-all strategy (HTTP API)";

    public EtcdSyncPlugin(ILogger<EtcdSyncPlugin> logger)
    {
        _logger = logger;
    }

    public Task<SyncPluginResult> InitializeAsync(SyncPluginConfig config)
    {
        try
        {
            _config = config;

            var endpoints = config.Settings.GetValueOrDefault("endpoints", "http://localhost:2379");
            _keyPrefix = config.Settings.GetValueOrDefault("keyPrefix", "/agileconfig");

            _logger.LogInformation("Initializing Etcd plugin with endpoints: {Endpoints}", endpoints);

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(endpoints.TrimEnd('/'))
            };

            return Task.FromResult(new SyncPluginResult { Success = true, Message = "Initialized" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Etcd plugin");
            return Task.FromResult(new SyncPluginResult { Success = false, Message = ex.Message, Exception = ex });
        }
    }

    /// <summary>
    /// Full sync: delete all + insert all
    /// </summary>
    public async Task<SyncPluginResult> SyncAllAsync(SyncContext[] contexts)
    {
        if (contexts == null || contexts.Length == 0)
        {
            _logger.LogInformation("No configs to sync");
            return new SyncPluginResult { Success = true, Message = "No configs to sync" };
        }

        try
        {
            var appId = contexts[0].AppId;
            var env = contexts[0].Env;
            var prefix = $"{_keyPrefix}/{appId}/{env}/";

            // Step 1: Delete all existing keys with prefix
            await DeleteRangeAsync(prefix);
            _logger.LogInformation("Deleted all keys with prefix {Prefix}", prefix);

            // Step 2: Insert all new configs
            foreach (var context in contexts)
            {
                var key = BuildKey(context);
                await PutAsync(key, context.Value);
            }

            _logger.LogInformation("Synced {Count} configs to etcd for app {AppId} env {Env}", 
                contexts.Length, appId, env);

            return new SyncPluginResult 
            { 
                Success = true, 
                Message = $"Synced {contexts.Length} configs" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync configs to etcd");
            return new SyncPluginResult { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    /// <summary>
    /// Put a key-value pair
    /// </summary>
    private async Task PutAsync(string key, string value)
    {
        var request = new
        {
            key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key)),
            value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
        };

        var response = await _httpClient.PostAsJsonAsync("/v3/kv/put", request);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Delete all keys with given prefix
    /// </summary>
    private async Task DeleteRangeAsync(string prefix)
    {
        // Range request to get all keys with prefix
        var rangeRequest = new
        {
            key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(prefix)),
            range_end = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(GetRangeEnd(prefix)))
        };

        var rangeResponse = await _httpClient.PostAsJsonAsync("/v3/kv/range", rangeRequest);
        rangeResponse.EnsureSuccessStatusCode();

        var rangeResult = await rangeResponse.Content.ReadFromJsonAsync<EtcdRangeResponse>();

        if (rangeResult?.kvs != null && rangeResult.kvs.Count > 0)
        {
            // Delete each key
            foreach (var kvp in rangeResult.kvs)
            {
                var deleteRequest = new
                {
                    key = kvp.key
                };

                var deleteResponse = await _httpClient.PostAsJsonAsync("/v3/kv/deleterange", deleteRequest);
                deleteResponse.EnsureSuccessStatusCode();
            }
        }
    }

    /// <summary>
    /// Get the range end for prefix deletion
    /// </summary>
    private string GetRangeEnd(string prefix)
    {
        // Increment the last character to create a range end
        var bytes = System.Text.Encoding.UTF8.GetBytes(prefix);
        bytes[bytes.Length - 1]++;
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public async Task<SyncPluginHealthResult> HealthCheckAsync()
    {
        try
        {
            // Try a simple range request to check connectivity
            var request = new
            {
                key = "YWJj", // "abc" in base64
                limit = 1
            };

            var response = await _httpClient.PostAsJsonAsync("/v3/kv/range", request);
            
            return new SyncPluginHealthResult
            {
                Healthy = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "Etcd connection OK" : "Etcd connection failed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etcd health check failed");
            return new SyncPluginHealthResult
            {
                Healthy = false,
                Message = ex.Message
            };
        }
    }

    public Task ShutdownAsync()
    {
        _logger.LogInformation("Etcd plugin shutdown");
        _httpClient?.Dispose();
        return Task.CompletedTask;
    }

    private string BuildKey(SyncContext context)
    {
        var group = string.IsNullOrEmpty(context.Group) ? "default" : context.Group;
        return $"{_keyPrefix}/{context.AppId}/{context.Env}/{group}/{context.Key}";
    }
}

/// <summary>
/// Response model for etcd range query
/// </summary>
internal class EtcdRangeResponse
{
    public int count { get; set; }
    public List<EtcdKv>? kvs { get; set; }
}

internal class EtcdKv
{
    public string key { get; set; } = "";
    public string value { get; set; } = "";
}
