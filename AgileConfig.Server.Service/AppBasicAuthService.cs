using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class AppBasicAuthService : IAppBasicAuthService
    {
        private readonly IAppService _appService;
        public AppBasicAuthService(IAppService appService)
        {
            _appService = appService;
        }
        /// <summary>
        /// 从request中解析出appid、secret
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public (string, string) GetAppIdSecret(HttpRequest httpRequest)
        {
            var authorization = httpRequest.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                return ("", "");
            }
            var authStr = authorization.First();
            //去掉basic_
            if (!authStr.StartsWith("Basic "))
            {
                return ("", ""); ;
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

            var appId = "";
            var sec = "";


            var baseAuthArr = base64Str.Split(':');

            if (baseAuthArr.Length > 0)
            {
                appId = baseAuthArr[0];
            }
            if (baseAuthArr.Length > 1)
            {
                sec = baseAuthArr[1];
            }

            return (appId, sec);
        }

        public async Task<bool> ValidAsync(HttpRequest httpRequest)
        {
            var appIdSecret = GetAppIdSecret(httpRequest);
            var appId = appIdSecret.Item1;
            var sec = appIdSecret.Item2;
            if (string.IsNullOrEmpty(appIdSecret.Item1))
            {
                return false;
            }

            var app = await _appService.GetAsync(appId);
            if (app == null)
            {
                return false;
            }
            if (!app.Enabled)
            {
                return false;
            }

            var txt = $"{app.Id}:{app.Secret}";

            return txt == $"{appId}:{sec}";
        }
    }
}
