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
    private bool _allowOverwriteOtherData = false;
    private int _maxTxnOperations = 500;
    private string _syncStrategy = "FullReplace";

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
            _keyPrefix = config.Settings.GetValueOrDefault("keyPrefix", "/agileconfig").TrimEnd('/');
            _allowOverwriteOtherData = bool.TryParse(config.Settings.GetValueOrDefault("allowOverwriteOtherData", "false"), out var a) && a;
            _maxTxnOperations = int.TryParse(config.Settings.GetValueOrDefault("maxTxnOperations", "500"), out var m) && m > 0 ? m : 500;
            _syncStrategy = config.Settings.GetValueOrDefault("syncStrategy", "FullReplace");

            // Validate key prefix
            if (_keyPrefix == "/" || _keyPrefix.Length < 5 || !_keyPrefix.StartsWith('/'))
            {
                var error = $"Invalid key prefix '{_keyPrefix}'. Prefix must start with '/' and be at least 5 characters long, cannot be root path '/'.";
                _logger.LogError(error);
                return Task.FromResult(new SyncPluginResult { Success = false, Message = error });
            }

            _logger.LogInformation("Initializing Etcd plugin with endpoints: {Endpoints}, keyPrefix: {KeyPrefix}, allowOverwriteOtherData: {AllowOverwrite}, maxTxnOperations: {MaxTxn}, syncStrategy: {SyncStrategy}", 
                endpoints, _keyPrefix, _allowOverwriteOtherData, _maxTxnOperations, _syncStrategy);

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
    /// Full sync: use etcd transaction to atomically replace all configs
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

            List<string>? existingKeys = null;
            // Safety check: verify existing keys are valid AgileConfig keys if overwrite is not allowed
            if (!_allowOverwriteOtherData)
            {
                existingKeys = await GetRangeKeysAsync(prefix);
                foreach (var base64Key in existingKeys)
                {
                    var key = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Key));
                    // Valid key format: {prefix}/{appId}/{env}/{group}/{key}
                    var relativePath = key.Substring(prefix.Length);
                    if (string.IsNullOrEmpty(relativePath) || !relativePath.Contains('/') || relativePath.StartsWith('/') || relativePath.EndsWith('/'))
                    {
                        var error = $"Found invalid key '{key}' under prefix '{prefix}' that does not match AgileConfig format. To overwrite anyway, set 'allowOverwriteOtherData' to true.";
                        _logger.LogError(error);
                        return new SyncPluginResult { Success = false, Message = error };
                    }
                }
            }

            List<object> operations;
            int deletedCount = 0;
            int addedCount = 0;
            int updatedCount = 0;

            if (_syncStrategy.Equals("Incremental", StringComparison.OrdinalIgnoreCase))
            {
                // Incremental sync: compare existing configs with new ones
                var existingKvs = await GetRangeKvsAsync(prefix);
                var existingDict = existingKvs.ToDictionary(
                    kv => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(kv.key)),
                    kv => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(kv.value))
                );

                var newDict = contexts.ToDictionary(
                    c => BuildKey(c),
                    c => c.Value
                );

                operations = new List<object>();

                // Find keys to delete (exist in etcd but not in new configs)
                foreach (var kv in existingDict)
                {
                    if (!newDict.ContainsKey(kv.Key))
                    {
                        operations.Add(new
                        {
                            request_delete_range = new
                            {
                                key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(kv.Key))
                            }
                        });
                        deletedCount++;
                    }
                }

                // Find keys to add/update
                foreach (var kv in newDict)
                {
                    if (!existingDict.TryGetValue(kv.Key, out var existingValue) || existingValue != kv.Value)
                    {
                        operations.Add(new
                        {
                            request_put = new
                            {
                                key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(kv.Key)),
                                value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(kv.Value))
                            }
                        });
                        if (!existingDict.ContainsKey(kv.Key))
                            addedCount++;
                        else
                            updatedCount++;
                    }
                }

                _logger.LogInformation("Incremental sync: {Deleted} to delete, {Added} to add, {Updated} to update", 
                    deletedCount, addedCount, updatedCount);
            }
            else
            {
                // Full replace sync: delete all then insert all
                operations = new List<object>
                {
                    // Add single delete_range operation to delete all existing keys with prefix
                    new
                    {
                        request_delete_range = new
                        {
                            key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(prefix)),
                            range_end = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(GetRangeEnd(prefix)))
                        }
                    }
                };
                existingKeys ??= await GetRangeKeysAsync(prefix);
                deletedCount = existingKeys.Count;

                // Add put operations for all new configs
                foreach (var context in contexts)
                {
                    var key = BuildKey(context);
                    operations.Add(new
                    {
                        request_put = new
                        {
                            key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key)),
                            value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(context.Value))
                        }
                    });
                    addedCount++;
                }

                _logger.LogInformation("Full replace sync: {Deleted} old keys to delete, {Added} new keys to add", 
                    deletedCount, addedCount);
            }

            // Split operations into batches and execute
            var batchSize = _maxTxnOperations;
            var totalBatches = (int)Math.Ceiling((double)operations.Count / batchSize);
            var processed = 0;

            for (var i = 0; i < totalBatches; i++)
            {
                var batch = operations.Skip(i * batchSize).Take(batchSize).ToList();
                try
                {
                    var batchTxn = new
                    {
                        success = batch
                    };

                    var batchResponse = await _httpClient.PostAsJsonAsync("/v3/kv/txn", batchTxn);
                    batchResponse.EnsureSuccessStatusCode();
                    processed += batch.Count;
                    _logger.LogDebug("Processed batch {Current}/{Total}, {Processed} operations completed", i + 1, totalBatches, processed);
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("request too large") && batchSize > 10)
                {
                    // Auto reduce batch size on request too large error
                    batchSize = Math.Max(10, batchSize / 2);
                    _logger.LogWarning("Request too large, reducing batch size to {BatchSize} and retrying batch", batchSize);
                    i--; // Retry current batch
                }
            }

            _logger.LogInformation("Successfully synced configs to etcd for app {AppId} env {Env}: {Deleted} deleted, {Added} added, {Updated} updated in {Batches} batches", 
                appId, env, deletedCount, addedCount, updatedCount, totalBatches);

            return new SyncPluginResult 
            { 
                Success = true, 
                Message = $"Synced: {deletedCount} deleted, {addedCount} added, {updatedCount} updated in {totalBatches} batches" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync configs to etcd (transaction rolled back, no changes applied)");
            return new SyncPluginResult { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    /// <summary>
    /// Get all keys with given prefix
    /// </summary>
    private async Task<List<string>> GetRangeKeysAsync(string prefix)
    {
        var keys = new List<string>();
        var rangeRequest = new
        {
            key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(prefix)),
            range_end = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(GetRangeEnd(prefix))),
            keys_only = true
        };

        var rangeResponse = await _httpClient.PostAsJsonAsync("/v3/kv/range", rangeRequest);
        rangeResponse.EnsureSuccessStatusCode();
        
        var rangeResult = await rangeResponse.Content.ReadFromJsonAsync<EtcdRangeResponse>();
        
        if (rangeResult?.kvs != null)
        {
            keys.AddRange(rangeResult.kvs.Select(kv => kv.key));
        }

        return keys;
    }

    /// <summary>
    /// Get all key-value pairs with given prefix
    /// </summary>
    private async Task<List<EtcdKv>> GetRangeKvsAsync(string prefix)
    {
        var rangeRequest = new
        {
            key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(prefix)),
            range_end = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(GetRangeEnd(prefix)))
        };

        var rangeResponse = await _httpClient.PostAsJsonAsync("/v3/kv/range", rangeRequest);
        rangeResponse.EnsureSuccessStatusCode();

        var rangeResult = await rangeResponse.Content.ReadFromJsonAsync<EtcdRangeResponse>();
        
        return rangeResult?.kvs ?? new List<EtcdKv>();
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
