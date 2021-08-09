using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Utilites
{
    public static class ClaimExt
    {
        public static string GetCurrentUserName(this Controller ctrl)
        {
            return ctrl.HttpContext.GetUserNameFromClaim();
        }

        public static string GetCurrentUserId(this Controller ctrl)
        {
            return ctrl.HttpContext.GetUserIdFromClaim();
        }

        public static string GetUserNameFromClaim(this HttpContext httpContext)
        {
            return httpContext.User.FindFirst("name")?.Value;
        }

        public static string GetUserIdFromClaim(this HttpContext httpContext)
        {
            return httpContext.User.FindFirst("id")?.Value;
        }
    }
}
