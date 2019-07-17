using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IConfigService
    {
        Task<Config> GetAsync(string id);

        Task<Config> GetByAppIdKey(string appId, string group, string key);

        Task<List<Config>> GetByAppId(string appId);

        Task<bool> AddAsync(Config app);

        Task<bool> DeleteAsync(Config app);

        Task<bool> DeleteAsync(string appId);

        Task<bool> UpdateAsync(Config app);

        Task<List<Config>> GetAllConfigsAsync();
    }
}
