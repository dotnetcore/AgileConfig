using AgileConfig.Server.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IConfigService
    {
        Task<Config> GetAsync(string id);

        Task<Config> GetByAppIdKey(string appId, string group, string key);
        /// <summary>
        /// 根据appId,group,key查询配置，其中group，key使用like查询
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<Config>> Search(string appId, string group, string key);
        Task<List<Config>> GetByAppId(string appId);
        Task<List<Config>> GetPublishedConfigsByAppId(string appId);
        /// <summary>
        /// 获取app相关的配置继承的app的配置一并查出
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<List<Config>> GetPublishedConfigsByAppIdWithInheritanced(string appId);
        /// <summary>
        /// 获取app的配置项继承的app配置合并进来转换为字典
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<Dictionary<string, Config>> GetPublishedConfigsByAppIdWithInheritanced_Dictionary(string appId);
        Task<bool> AddAsync(Config config);

        Task<bool> AddRangeAsync(List<Config> configs);

        Task<bool> DeleteAsync(Config app);

        Task<bool> DeleteAsync(string appId);

        Task<bool> UpdateAsync(Config app);

        Task<List<Config>> GetAllConfigsAsync();

        Task<int> CountEnabledConfigsAsync();

        /// <summary>
        /// 计算已发布配置项的MD5
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5(string appId);
        /// <summary>
        /// 计算已发布配置项的MD5 合并继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5WithInheritanced(string appId);
        
        /// <summary>
        /// 计算已发布配置项的MD5进行缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5Cache(string appId);

        /// <summary>
        /// 计算已发布配置项的MD5进行缓存 合并继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5CacheWithInheritanced(string appId);

        /// <summary>
        /// 构造key
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        string GenerateKey(Config config);
    }
}
