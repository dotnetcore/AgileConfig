using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Mongodb.Service;

public class AdmBasicAuthService(IUserService userService, ILoggerFactory lf) : IAdmBasicAuthService
{
    private readonly ILogger _logger = lf.CreateLogger<AdmBasicAuthService>();

    public async Task<bool> ValidAsync(HttpRequest httpRequest)
    {
        var userPassword = httpRequest.GetUserNamePasswordFromBasicAuthorization();
        if (string.IsNullOrEmpty(userPassword.Item1)||string.IsNullOrEmpty(userPassword.Item2))
        {
            _logger.LogWarning("Basic auth header have no username or password");
            return false;
        }

        var result = await userService.ValidateUserPassword(userPassword.Item1, userPassword.Item2);
        return result;
    }

    public (string, string) GetUserNamePassword(HttpRequest httpRequest)
    {
        return httpRequest.GetUserNamePasswordFromBasicAuthorization();
    }
}