using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public class AdmBasicAuthService : IAdmBasicAuthService
{
    private readonly ILogger _logger;
    private readonly IUserService _userService;

    public AdmBasicAuthService(IUserService userService, ILoggerFactory lf)
    {
        _logger = lf.CreateLogger<AdmBasicAuthService>();
        _userService = userService;
    }

    public (string, string) GetUserNamePassword(HttpRequest httpRequest)
    {
        return httpRequest.GetUserNamePasswordFromBasicAuthorization();
    }

    public Task<bool> ValidAsync(HttpRequest httpRequest)
    {
        var userPassword = httpRequest.GetUserNamePasswordFromBasicAuthorization();
        if (string.IsNullOrEmpty(userPassword.Item1) || string.IsNullOrEmpty(userPassword.Item2))
        {
            _logger.LogWarning("Basic auth header have no username or password .");
            return Task.FromResult(false);
        }

        return _userService.ValidateUserPassword(userPassword.Item1, userPassword.Item2);
    }
}