using System.Threading.Tasks;
using System.Threading;

namespace AgileConfig.Server.IService;

public interface IServiceHealthCheckService
{
    Task CheckAsync(CancellationToken cancellationToken);
}