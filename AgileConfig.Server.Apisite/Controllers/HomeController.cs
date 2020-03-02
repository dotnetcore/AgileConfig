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
        public IActionResult Index()
        {
            if ("true".Equals(Configuration.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase))
            {
                return View();
            }

            return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
        }
        public IActionResult GetView(string viewName)
        {
            if ("true".Equals(Configuration.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase))
            {
                return View(viewName);
            }

            return Content("");
        }

        public IActionResult Echo()
        {
            return Content("ok");
        }
    }
}
