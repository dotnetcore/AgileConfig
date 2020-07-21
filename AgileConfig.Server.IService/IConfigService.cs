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
        /// 计算已发布配置项的MD5进行缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<string> AppPublishedConfigsMd5Cache(string appId);
    }
}
