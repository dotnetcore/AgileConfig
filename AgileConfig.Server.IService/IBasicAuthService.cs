using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IBasicAuthService
    {
        Task<bool> ValidAsync(HttpRequest httpRequest);
    }
}
