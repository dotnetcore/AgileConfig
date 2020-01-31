using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IConfigService : IDisposable
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

        Task<bool> AddAsync(Config app);

        Task<bool> DeleteAsync(Config app);

        Task<bool> DeleteAsync(string appId);

        Task<bool> UpdateAsync(Config app);

        Task<List<Config>> GetAllConfigsAsync();
    }
}
