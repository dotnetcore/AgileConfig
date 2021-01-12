using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.IService
{
    /// <summary>
    /// 基于管理员的认证
    /// </summary>
    public interface IAdmBasicAuthService : IBasicAuthService
    {
        string GetPassword(HttpRequest httpRequest);
    }
}