using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class AdmBasicAuthService : IAdmBasicAuthService
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        public AdmBasicAuthService(IUserService userService, ILoggerFactory lf)
        {
            _logger = lf.CreateLogger<AdmBasicAuthService>();
            _userService = userService;
        }

        public (string, string) GetUserNamePassword(HttpRequest httpRequest)
        {
            return httpRequest.GetUserNamePasswordFromBasicAuthorization();
        }

        public async Task<bool> ValidAsync(HttpRequest httpRequest)
        {
            var userPassword = httpRequest.GetUserNamePasswordFromBasicAuthorization();
            if (string.IsNullOrEmpty(userPassword.Item1)||string.IsNullOrEmpty(userPassword.Item2))
            {
                _logger.LogWarning("Basic auth header have no username or password .");
                return false;
            }

            var result = await _userService.ValidateUserPassword(userPassword.Item1, userPassword.Item2);
            return result;
        }
    }
}
