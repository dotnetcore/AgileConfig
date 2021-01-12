using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class AdmBasicAuthService : IAdmBasicAuthService
    {
        private readonly ISettingService _settingService;
        public AdmBasicAuthService(ISettingService settingService)
        {
            _settingService = settingService;
        }
  
        /// <summary>
        /// 从request中解析出password
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public string GetPassword(HttpRequest httpRequest)
        {
            var authorization = httpRequest.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                return "";
            }
            var authStr = authorization.First();
            //去掉basic_
            if (!authStr.StartsWith("Basic "))
            {
                return "";
            }
            authStr = authStr.Substring(6, authStr.Length - 6);
            byte[] base64Decode = null;
            try
            {
                base64Decode = Convert.FromBase64String(authStr);
            }
            catch
            {
                return "";
            }
            var base64Str = Encoding.UTF8.GetString(base64Decode);

            if (string.IsNullOrEmpty(base64Str))
            {
                return "";
            }

            var password = "";
            var baseAuthArr = base64Str.Split(':');
          
            if (baseAuthArr.Length > 1)
            {
                password = baseAuthArr[1];
            }

            return password;
        }

        public async Task<bool> ValidAsync(HttpRequest httpRequest)
        {
            var password = GetPassword(httpRequest);
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            var result = await _settingService.ValidateAdminPassword(password);
            return result;
        }
    }
}
