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
        public async Task<IActionResult> Index()
        {
            if (!IsAdminConsoleMode)
            {
                return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
            }

            var auth = (await HttpContext.AuthenticateAsync()).Succeeded;
            if (auth)
            {
                return View();
            }
            else
            {
                return Redirect("/Admin/Login");
            }

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
