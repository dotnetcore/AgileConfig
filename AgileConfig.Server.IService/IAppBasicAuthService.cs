using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IAppBasicAuthService
    {
        (string, string) GetAppIdSecret(HttpRequest httpRequest);
        Task<bool> ValidAsync(HttpRequest httpRequest);
    }
}