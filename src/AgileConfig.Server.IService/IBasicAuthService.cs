using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.IService;

public interface IBasicAuthService
{
    Task<bool> ValidAsync(HttpRequest httpRequest);
}