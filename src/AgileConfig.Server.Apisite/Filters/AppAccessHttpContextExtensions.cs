using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Apisite.Filters;

public static class AppAccessHttpContextExtensions
{
    private static readonly object ItemKey = typeof(UserAppAccess);

    public static void SetUserAppAccess(this HttpContext httpContext, UserAppAccess access)
    {
        httpContext.Items[ItemKey] = access;
    }

    /// <summary>
    ///     The application access resolved by <see cref="PermissionCheckAttribute" /> for the current request.
    ///     Denies every application when the action did not declare an <see cref="AppScope" />.
    /// </summary>
    public static UserAppAccess GetUserAppAccess(this HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(ItemKey, out var access) && access is UserAppAccess userAppAccess
            ? userAppAccess
            : UserAppAccess.None;
    }
}
