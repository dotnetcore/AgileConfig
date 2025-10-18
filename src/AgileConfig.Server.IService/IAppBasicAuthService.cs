using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.IService
{
    /// <summary>
    /// Provides basic authentication based on application credentials.
    /// </summary>
    public interface IAppBasicAuthService: IBasicAuthService
    {
        (string, string) GetAppIdSecret(HttpRequest httpRequest);
    }
}