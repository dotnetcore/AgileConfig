using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.IService;

/// <summary>
///     Provides basic authentication based on administrator credentials.
/// </summary>
public interface IAdmBasicAuthService : IBasicAuthService
{
    (string, string) GetUserNamePassword(HttpRequest httpRequest);
}