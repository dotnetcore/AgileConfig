using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class HomeController : Controller
    {
        IConfigService ConfigService;
        IAppService AppService;
        public HomeController(IConfigService configService, IAppService appService)
        {
            AppService = appService;
            ConfigService = configService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetView(string viewName)
        {
            return View(viewName);
        }

        public async Task<IActionResult> Report()
        {
            var report = new ServerStatusReport();
            report.WebsocketCollectionReport = Websocket.WebsocketCollection.Instance.Report();
            report.AppCount = await AppService.CountEnabledAppsAsync();
            report.ConfigCount = await ConfigService.CountEnabledConfigsAsync();

            return Json(new
            {
                success = true,
                data = report
            });
        }
    }
}
