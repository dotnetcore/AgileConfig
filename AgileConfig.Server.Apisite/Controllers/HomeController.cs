using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
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

            return Content($"AgileConfig node running now , {DateTime.Now.ToLongTimeString()}");
        }
        public IActionResult GetView(string viewName)
        {
            if ("true".Equals(Configuration.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase))
            {
                return View(viewName);
            }

            return Content("");
        }
    }
}
