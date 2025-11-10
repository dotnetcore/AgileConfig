using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly ISystemInitializationService _systemInitializationService;
    private readonly IUserService _userService;

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
            return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");

        if (!_systemInitializationService.HasSa()) return Redirect(Request.PathBase + "/ui#/user/initpassword");

        return Redirect(Request.PathBase + "/ui");
    }

    public async Task<IActionResult> Current()
    {
        var userName = this.GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
            return Json(new
            {
                currentUser = new
                {
                }
            });

        var userId = await this.GetCurrentUserId(_userService);
        var userRoles = await _userService.GetUserRolesAsync(userId);
        var userFunctions = await _permissionService.GetUserPermission(userId);
        var userCategories = await _permissionService.GetUserCategories(userId);

        return Json(new
        {
            currentUser = new
            {
                userId,
                userName,
                currentAuthority = userRoles.Select(r => r.Name),
                currentFunctions = userFunctions,
                currentCategories = userCategories
            }
        });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Sys()
    {
        var appVer = Assembly.GetAssembly(typeof(Program))?.GetName()?.Version?.ToString();
        var userName = this.GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
            return Json(new
            {
                appVer,
                passwordInited = _systemInitializationService.HasSa(),
                ssoEnabled = Appsettings.SsoEnabled,
                ssoButtonText = Appsettings.SsoButtonText
            });

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
        return Content(string.Join(',', IpExt.GetEndpointIp()));
    }
}