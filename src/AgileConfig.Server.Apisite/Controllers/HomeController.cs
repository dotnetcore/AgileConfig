using System;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using AgileConfig.Server.Apisite.Utilites;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly ISystemInitializationService _systemInitializationService;

        public HomeController(
            ISettingService settingService,
            IUserService userService,
            IPermissionService permissionService,
            ISystemInitializationService systemInitializationService
            )
        {
            _settingService = settingService;
            _userService = userService;
            _permissionService = permissionService;
            _systemInitializationService = systemInitializationService;
        }

        [AllowAnonymous]
        public IActionResult IndexAsync()
        {
            if (!Appsettings.IsAdminConsoleMode)
            {
                return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
            }

            if (!_systemInitializationService.HasSa())
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
                    currentAuthority = userRoles.Select(r => r.Code),
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
                    passwordInited = _systemInitializationService.HasSa(),
                    ssoEnabled = Appsettings.SsoEnabled,
                    ssoButtonText = Appsettings.SsoButtonText
                });
            }

            var envList = await _settingService.GetEnvironmentList();
            return Json(new
            {
                appVer,
                passwordInited = _systemInitializationService.HasSa(),
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
            return Content(String.Join(',', IpExt.GetEndpointIp()));
        }
    }
}
