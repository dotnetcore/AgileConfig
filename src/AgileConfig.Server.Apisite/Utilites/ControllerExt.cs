using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Utilites
{
    public static class ControllerExt
    {
        /// <summary>
        /// 获取用户名，优先从Claim获取，没有从Basic认证的头部取
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static string GetCurrentUserName(this Controller ctrl)
        {
            var name = ctrl.HttpContext.GetUserNameFromClaim();
            if (string.IsNullOrEmpty(name))
            {
                var result = ctrl.Request.GetUserNamePasswordFromBasicAuthorization();
                name = result.Item1;
            }

            return name;
        }

        /// <summary>
        /// 获取用户Id，优先从Claim获取，没有从Basic认证的头部取用户名后从数据库查Id
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static async Task<string> GetCurrentUserId(this Controller ctrl, IUserService userservice = null)
        {
            var userId = ctrl.HttpContext.GetUserIdFromClaim();
            if (string.IsNullOrEmpty(userId))
            {
                var result = ctrl.Request.GetUserNamePasswordFromBasicAuthorization();
                if (!string.IsNullOrEmpty(result.Item1))
                {
                    var user =(await userservice.GetUsersByNameAsync(result.Item1)).FirstOrDefault(x=>x.Status == Data.Entity.UserStatus.Normal);
                    userId = user?.Id;
                }
            }

            return userId;
        }

    }
}
