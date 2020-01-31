using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IAppService : IDisposable
    {
        Task<App> GetAsync(string id);
        Task<bool> AddAsync(App app);

        Task<bool> DeleteAsync(App app);

        Task<bool> DeleteAsync(string appId);

        Task<bool> UpdateAsync(App app);

        Task<List<App>> GetAllAppsAsync();
    }
}
