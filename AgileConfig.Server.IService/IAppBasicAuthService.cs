using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.IService
{
    /// <summary>
    /// 基于应用的认证
    /// </summary>
    public interface IAppBasicAuthService: IBasicAuthService
    {
        (string, string) GetAppIdSecret(HttpRequest httpRequest);
    }
}