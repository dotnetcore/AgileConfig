using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IAppService
    {
        Task<bool> AddAsync(App app);

        Task<bool> Delete(App app);

        Task<bool> Delete(string appId);

        Task<List<App>> GetAllAppsAsync();
    }
}
