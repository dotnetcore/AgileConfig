using System;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.OIDC;

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
            IPermissionService permissionService
            )
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
                return Redirect(Request.PathBase + "/ui#/user/initpassword");
            }

            return Redirect(Request.PathBase + "/ui");
        }

        public async Task<IActionResult> Current()
        {
            string userName = this.GetCurrentUserName();
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new
                {
                    currentUser = new
                    {
                    }
                });
            }

            string userId = await this.GetCurrentUserId(_userService);
            var userRoles = await _userService.GetUserRolesAsync(userId);
            var userFunctions = await _permissionService.GetUserPermission(userId);

            return Json(new
            {
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
        public async Task<IActionResult> Sys()
        {
            string appVer = Assembly.GetAssembly(typeof(Program))?.GetName()?.Version?.ToString();
            string userName = this.GetCurrentUserName();
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new
                {
                    appVer,
                    passwordInited = await _settingService.HasSuperAdmin(),
                    ssoEnabled = Appsettings.SsoEnabled,
                    ssoButtonText = Appsettings.SsoButtonText
                });
            }

            var envList = await _settingService.GetEnvironmentList();
            return Json(new
            {
                appVer,
                passwordInited = await _settingService.HasSuperAdmin(),
                envList,
                ssoEnabled = Appsettings.SsoEnabled,
                ssoButtonText = Appsettings.SsoButtonText
            });
        }

        [AllowAnonymous]
        public IActionResult Echo()
        {
            return Content("ok");
        }


        [AllowAnonymous]
        public IActionResult GetIP()
        {
            return Content(String.Join(',', IPExt.GetEndpointIp()));
        }
    }
}
