using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers
{
    public static class ControllerExt
    {
        public static string GetCurrentUserName(this Controller ctrl)
        {
            return ctrl.HttpContext.User.FindFirst("name")?.Value;
        }

        public static string GetCurrentUserId(this Controller ctrl)
        {
            return ctrl.HttpContext.User.FindFirst("id")?.Value;
        }
    }
}
