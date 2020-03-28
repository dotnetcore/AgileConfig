using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISettingService _settingService;
        public HomeController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task<IActionResult> Index()
        {
            if (IsAdminConsoleMode)
            {
                if (!await _settingService.HasAdminPassword())
                {
                    return View("init_password");
                }
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

        public IActionResult Echo()
        {
            return Content("ok");
        }

        private bool IsAdminConsoleMode => "true".Equals(Configuration.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase);
    }
}
