using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IConfigService: IDisposable
    {
        /// <summary>
        /// Publish the current pending configuration items. When ids are provided, only those items are published; otherwise all pending items are published.
        /// </summary>
        /// <param name="appId">Application ID whose configuration should be published.</param>
        /// <param name="ids">Specific configuration identifiers to publish, or null to publish all pending items.</param>
        /// <param name="log">Publish log message.</param>
        /// <param name="operatorr">User name of the operator performing the publish.</param>
        /// <param name="env">Environment in which to publish.</param>
        /// <returns>Publish success indicator and resulting publish timeline identifier.</returns>
        Task<(bool result, string publishTimelineId)> Publish(string appId,string[] ids, string log, string operatorr, string env);

        Task<Config> GetAsync(string id, string env);

        Task<Config> GetByAppIdKeyEnv(string appId, string group, string key, string env);
        /// <summary>
        /// Query configuration entries by appId, group, and key; group and key use LIKE matching.
        /// </summary>
        /// <param name="appId">Application ID whose configurations should be searched.</param>
        /// <param name="group">Group name filter, supporting partial matches.</param>
        /// <param name="key">Configuration key filter, supporting partial matches.</param>
        /// <param name="env">Environment from which to query configuration.</param>
        /// <returns>List of configuration entries matching the filter.</returns>
        Task<List<Config>> Search(string appId, string group, string key, string env);
        Task<List<Config>> GetByAppIdAsync(string appId, string env);

        /// <summary>
        /// Retrieve the published configuration for the app together with inherited app configuration.
        /// </summary>
        /// <param name="appId">Application ID whose published configuration should be retrieved.</param>
        /// <param name="env">Environment from which to load the configuration.</param>
        /// <returns>List of configuration entries merged with inherited apps.</returns>
        Task<List<Config>> GetPublishedConfigsByAppIdWithInheritance(string appId, string env);
        /// <summary>
        /// Retrieve the app configuration merged with inherited configuration and convert it into a dictionary.
        /// </summary>
        /// <param name="appId">Application ID whose published configuration should be retrieved.</param>
        /// <param name="env">Environment from which to load the configuration.</param>
        /// <returns>Dictionary of configuration entries keyed by generated configuration key.</returns>
        Task<Dictionary<string, Config>> GetPublishedConfigsByAppIdWithInheritance_Dictionary(string appId, string env);
        Task<bool> AddAsync(Config config, string env);

        Task<bool> AddRangeAsync(List<Config> configs, string env);

        Task<bool> DeleteAsync(Config config, string env);

        Task<bool> DeleteAsync(string configId, string env);

        Task<bool> UpdateAsync(Config config, string env);

        Task<bool> UpdateAsync(List<Config> configs, string env);

        /// <summary>
        /// Cancel the edit status.
        /// </summary>
        /// <param name="ids">Configuration identifiers to revert.</param>
        /// <param name="env">Environment in which the configurations reside.</param>
        /// <returns>True when the edit status is successfully cleared.</returns>
        Task<bool> CancelEdit(List<string> ids, string env);

        Task<List<Config>> GetAllConfigsAsync(string env);

        Task<int> CountEnabledConfigsAsync();

        /// <summary>
        /// Calculate the MD5 hash of the published configuration items.
        /// </summary>
        /// <param name="appId">Application ID whose published configuration should be hashed.</param>
        /// <param name="env">Environment providing the configuration.</param>
        /// <returns>MD5 hash of the published configuration.</returns>
        Task<string> AppPublishedConfigsMd5(string appId, string env);
        /// <summary>
        /// Calculate the MD5 hash of the published configuration merged with inherited app configuration.
        /// </summary>
        /// <param name="appId">Application ID whose inherited configuration should be hashed.</param>
        /// <param name="env">Environment providing the configuration.</param>
        /// <returns>MD5 hash of the merged configuration.</returns>
        Task<string> AppPublishedConfigsMd5WithInheritance(string appId, string env);

        /// <summary>
        /// Calculate and cache the MD5 hash of the published configuration items.
        /// </summary>
        /// <param name="appId">Application ID whose published configuration hash should be cached.</param>
        /// <param name="env">Environment associated with the cached hash.</param>
        /// <returns>MD5 hash of the published configuration.</returns>
        Task<string> AppPublishedConfigsMd5Cache(string appId, string env);

        /// <summary>
        /// Calculate and cache the MD5 hash of the published configuration merged with inherited app configuration.
        /// </summary>
        /// <param name="appId">Application ID whose inherited configuration hash should be cached.</param>
        /// <param name="env">Environment associated with the cached hash.</param>
        /// <returns>MD5 hash of the merged configuration.</returns>
        Task<string> AppPublishedConfigsMd5CacheWithInheritance(string appId, string env);

        /// <summary>
        /// Build the configuration key.
        /// </summary>
        /// <param name="config">Configuration item whose key should be generated.</param>
        /// <returns>Composite key string.</returns>
        string GenerateKey(Config config);

        /// <summary>
        /// Determine whether the configuration has been published.
        /// </summary>
        /// <param name="configId">Configuration identifier to check.</param>
        /// <param name="env">Environment in which to look for published configuration.</param>
        /// <returns>True when the configuration is published.</returns>
        Task<bool> IsPublishedAsync(string configId, string env);

        /// <summary>
        /// Retrieve publish details by the publish timeline identifier.
        /// </summary>
        /// <param name="publishTimelineId">Publish timeline identifier to query.</param>
        /// <param name="env">Environment in which the publish timeline exists.</param>
        /// <returns>List of publish details for the timeline.</returns>
        Task<List<PublishDetail>> GetPublishDetailByPublishTimelineIdAsync(string publishTimelineId, string env);

        /// <summary>
        /// Retrieve the publish timeline node.
        /// </summary>
        /// <param name="publishTimelineId">Publish timeline identifier.</param>
        /// <param name="env">Environment in which the publish timeline exists.</param>
        /// <returns>Publish timeline entity.</returns>
        Task<PublishTimeline> GetPublishTimeLineNodeAsync(string publishTimelineId, string env);

        /// <summary>
        /// Retrieve the publish history for the application.
        /// </summary>
        /// <param name="appId">Application ID whose publish history is requested.</param>
        /// <param name="env">Environment to query.</param>
        /// <returns>List of publish timeline entries.</returns>
        Task<List<PublishTimeline>> GetPublishTimelineHistoryAsync(string appId, string env);

        /// <summary>
        /// Retrieve the list of publish details for the application.
        /// </summary>
        /// <param name="appId">Application ID whose publish details are requested.</param>
        /// <param name="env">Environment to query.</param>
        /// <returns>List of publish detail records.</returns>
        Task<List<PublishDetail>> GetPublishDetailListAsync(string appId, string env);

        /// <summary>
        /// Retrieve the publish history for a specific configuration item.
        /// </summary>
        /// <param name="configId">Configuration identifier to query.</param>
        /// <param name="env">Environment to query.</param>
        /// <returns>List of publish detail records for the configuration.</returns>
        Task<List<PublishDetail>> GetConfigPublishedHistory(string configId, string env);

        /// <summary>
        /// Retrieve the currently published configuration items.
        /// </summary>
        /// <param name="appId">Application ID whose published configuration is requested.</param>
        /// <param name="env">Environment to query.</param>
        /// <returns>List of published configuration entries.</returns>
        Task<List<ConfigPublished>> GetPublishedConfigsAsync(string appId, string env);

        /// <summary>
        /// Retrieve a single published configuration item.
        /// </summary>
        /// <param name="configId">Configuration identifier to query.</param>
        /// <param name="env">Environment to query.</param>
        /// <returns>Published configuration entity.</returns>
        Task<ConfigPublished> GetPublishedConfigAsync(string configId, string env);

        /// <summary>
        /// Roll back to the configuration version at the specified publish timeline.
        /// </summary>
        /// <param name="publishTimelineId">Publish timeline identifier to restore.</param>
        /// <param name="env">Environment in which to perform the rollback.</param>
        /// <returns>True when the rollback succeeds.</returns>
        Task<bool> RollbackAsync(string publishTimelineId, string env);

        /// <summary>
        /// Synchronize configuration to target environments.
        /// </summary>
        /// <param name="appId">Application ID whose configuration should be synchronized.</param>
        /// <param name="currentEnv">Source environment.</param>
        /// <param name="toEnvs">Target environments.</param>
        /// <returns>True when synchronization succeeds.</returns>
        Task<bool> EnvSync(string appId, string currentEnv, List<string> toEnvs);

        /// <summary>
        /// Retrieve the configuration as a list of key/value pairs.
        /// </summary>
        /// <param name="appId">Application ID whose configuration is requested.</param>
        /// <param name="env">Environment to query.</param>
        /// <returns>List of key/value pairs representing configuration entries.</returns>
        Task<List<KeyValuePair<string, string>>> GetKvListAsync(string appId, string env);

        /// <summary>
        /// Convert the configuration represented as JSON into standard config entries and persist them.
        /// Compare with the existing configuration to mark additions, deletions, and updates.
        /// </summary>
        /// <param name="json">JSON string representing the configuration.</param>
        /// <param name="appId">Application ID.</param>
        /// <param name="env">Environment name.</param>
        /// <param name="isPatch">Indicates whether to apply patch mode updates.</param>
        /// <returns></returns>
        Task<bool> SaveJsonAsync(string json, string appId, string env, bool isPatch);

        /// <summary>
        /// Persist the key/value configuration list to the database.
        /// </summary>
        /// <param name="kvString">Serialized key/value pairs.</param>
        /// <param name="appId">Application ID that owns the configuration.</param>
        /// <param name="env">Environment where the configuration should be saved.</param>
        /// <param name="isPatch">Indicates whether to apply patch mode updates.</param>
        /// <returns>True when the key/value list is saved successfully.</returns>
        Task<bool> SaveKvListAsync(string kvString, string appId, string env, bool isPatch);

        /// <summary>
        /// Validate whether the key/value text is well-formed.
        /// </summary>
        /// <param name="kvStr">Text containing key/value pairs.</param>
        /// <returns>Tuple indicating whether validation passed and an error message.</returns>
        (bool, string) ValidateKvString(string kvStr);

        /// <summary>
        /// clear all cache
        /// </summary>
        void ClearCache();

        Task<string> GetLastPublishTimelineVirtualIdAsync(string appId, string env);

        Task<string> GetLastPublishTimelineVirtualIdAsyncWithCache(string appId, string env);

    }
}
