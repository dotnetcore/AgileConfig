using System;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISettingService _settingService;
        public HomeController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexAsync()
        {
            if (!Appsettings.IsAdminConsoleMode)
            {
                return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
            }

            if (!await _settingService.HasAdminPassword())
            {
                return Redirect("/ui#/user/initpassword");
            }

            return Redirect("/ui");
        }

        public async Task<IActionResult> SystemInfo()
        {
            string appVer = System.Reflection.Assembly.GetAssembly(typeof(AgileConfig.Server.Apisite.Program)).GetName().Version.ToString();

            return Json(new { 
                appVer,
                userName="admin",
                passwordInited=await _settingService.HasAdminPassword()
            });
        }

        [AllowAnonymous]
        public IActionResult Echo()
        {
            return Content("ok");
        }

    }
}
