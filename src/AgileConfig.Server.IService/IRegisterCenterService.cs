using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IRegisterCenterService
    {
        Task<string> RegisterAsync(ServiceInfo serviceInfo);

        Task<bool> UnRegisterAsync(string serviceUniqueId);
        
        Task<bool> UnRegisterByServiceIdAsync(string serviceId);

        Task<bool> ReceiveHeartbeatAsync(string serviceUniqueId);
    }
}
