using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IServiceInfoService: IDisposable
    {
        Task<ServiceInfo> GetByUniqueIdAsync(string id);
        Task UpdateServiceStatus(ServiceInfo service, ServiceAlive status);
        Task<List<ServiceInfo>> GetAllServiceInfoAsync();
        
        Task<List<ServiceInfo>> GetOnlineServiceInfoAsync();

        Task<List<ServiceInfo>> GetOfflineServiceInfoAsync();

        Task<string> ServicesMD5();

        Task<string> ServicesMD5Cache();

        void ClearCache();
    }
}
