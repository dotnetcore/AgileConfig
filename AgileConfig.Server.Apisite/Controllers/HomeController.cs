using System;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;

        public HomeController(
            ISettingService settingService, 
            IUserService userService,
            IPermissionService permissionService)
        {
            _settingService = settingService;
            _userService = userService;
            _permissionService = permissionService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexAsync()
        {
            if (!Appsettings.IsAdminConsoleMode)
            {
                return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
            }

            if (!await _settingService.HasSuperAdmin())
            {
                return Redirect("/ui#/user/initpassword");
            }

            return Redirect("/ui");
        }

        public async Task<IActionResult> SystemInfo()
        {
            string appVer = System.Reflection.Assembly.GetAssembly(typeof(AgileConfig.Server.Apisite.Program)).GetName().Version.ToString();
            string userName = HttpContext.User.FindFirst("name")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new
                {
                    appVer,
                    currentUser =new { 
                    }
                });
            }

            string userId = HttpContext.User.FindFirst("id")?.Value;
            var userRoles = await _userService.GetUserRolesAsync(userId);
            var userFunctions = await _permissionService.GetUserPermission(userId);

            return Json(new { 
                appVer,
                passwordInited=await _settingService.HasSuperAdmin(),
                currentUser = new
                {
                    userId = userId,
                    userName,
                    currentAuthority = userRoles.Select(r => r.ToString()),
                    currentFunctions = userFunctions
                }
            });
        }

        [AllowAnonymous]
        public IActionResult Echo()
        {
            return Content("ok");
        }

    }
}
