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
        private readonly IConfigService _configService;
        private readonly IAppService _appService;
        private readonly IServerNodeService _serverNodeService;

        public HomeController(IConfigService configService, IAppService appService, IServerNodeService serverNodeService)
        {
            _appService = appService;
            _configService = configService;
            _serverNodeService = serverNodeService;
        }
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

        [HttpPost]
        public async Task<IActionResult> Client_Offline(string id)
        {
            var client = WebsocketCollection.Instance.Get(id);
            if (client == null)
            {
                throw new Exception($"Can not find websocket client by id: {id}");
            }
            await WebsocketCollection.Instance.RemoveClient(client, System.Net.WebSockets.WebSocketCloseStatus.Empty);

            return Json(new
            {
                success = true,
            });
        }
    }
}
