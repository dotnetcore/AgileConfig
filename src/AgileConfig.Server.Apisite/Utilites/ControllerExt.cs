using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Utilites;

public static class ControllerExt
{
    /// <summary>
    ///     Retrieve the current user name, preferring claims and falling back to basic authentication.
    /// </summary>
    /// <param name="ctrl">Controller instance.</param>
    /// <returns>User name of the current principal.</returns>
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
    ///     Retrieve the current user ID, preferring claims and falling back to a database lookup using basic authentication.
    /// </summary>
    /// <param name="ctrl">Controller instance.</param>
    /// <param name="userService">User service used to resolve credentials when claims are absent.</param>
    /// <returns>User identifier of the current principal.</returns>
    public static async Task<string> GetCurrentUserId(this Controller ctrl, IUserService userService)
    {
        var userId = ctrl.HttpContext.GetUserIdFromClaim();
        if (string.IsNullOrEmpty(userId))
        {
            var result = ctrl.Request.GetUserNamePasswordFromBasicAuthorization();
            if (!string.IsNullOrEmpty(result.Item1))
            {
                var user = (await userService.GetUsersByNameAsync(result.Item1)).FirstOrDefault(x =>
                    x.Status == UserStatus.Normal);
                userId = user?.Id;
            }
        }

        return userId;
    }
}