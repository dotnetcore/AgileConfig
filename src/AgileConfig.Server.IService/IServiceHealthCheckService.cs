using System.Threading.Tasks;

namespace AgileConfig.Server.IService;

public interface IServiceHealthCheckService
{
    Task StartCheckAsync();
}