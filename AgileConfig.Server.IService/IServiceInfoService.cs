using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IServiceInfoService: IDisposable
    {
        Task<List<ServiceInfo>> GetAllServiceInfoAsync();
    }
}
