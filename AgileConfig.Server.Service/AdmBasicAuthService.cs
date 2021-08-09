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
  
        /// <summary>
        /// 从request中解析出username, password
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public (string, string) GetUserNamePassword(HttpRequest httpRequest)
        {
            var authorization = httpRequest.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                return ("","");
            }
            var authStr = authorization.First();
            //去掉basic_
            if (!authStr.StartsWith("Basic "))
            {
                return ("", "");
            }
            authStr = authStr.Substring(6, authStr.Length - 6);
            byte[] base64Decode = null;
            try
            {
                base64Decode = Convert.FromBase64String(authStr);
            }
            catch
            {
                return ("", "");
            }
            var base64Str = Encoding.UTF8.GetString(base64Decode);

            if (string.IsNullOrEmpty(base64Str))
            {
                return ("", "");
            }

            var userName = "";
            var password = "";
            var baseAuthArr = base64Str.Split(':');

            if (baseAuthArr.Length > 0)
            {
                userName = baseAuthArr[0];
            }          
            if (baseAuthArr.Length > 1)
            {
                password = baseAuthArr[1];
            }

            return (userName, password);
        }

        public async Task<bool> ValidAsync(HttpRequest httpRequest)
        {
            var userPassword = GetUserNamePassword(httpRequest);
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
