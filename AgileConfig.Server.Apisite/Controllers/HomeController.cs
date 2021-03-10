using System;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authentication;
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
            if (!IsAdminConsoleMode)
            {
                return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
            }

            if (!await _settingService.HasAdminPassword())
            {
                return Redirect("/admin/InitPassword");
            }

            return Redirect("/index.html");
        }
        public IActionResult GetView(string viewName)
        {
            if (IsAdminConsoleMode)
            {
                return View(viewName);
            }

            return Content("");
        }

        [AllowAnonymous]
        public IActionResult Echo()
        {
            return Content("ok");
        }

        private bool IsAdminConsoleMode => "true".Equals(Global.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase);
    }
}
