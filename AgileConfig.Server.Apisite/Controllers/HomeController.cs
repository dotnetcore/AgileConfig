using System;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Index()
        {
            if (IsAdminConsoleMode)
            {
                return View();
            }

            return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
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

        private bool IsAdminConsoleMode => "true".Equals(Configuration.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase);
    }
}
