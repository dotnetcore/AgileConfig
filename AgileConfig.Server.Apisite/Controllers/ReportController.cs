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
    public class ReportController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IAppService _appService;
        private readonly IServerNodeService _serverNodeService;

        public ReportController(IConfigService configService, IAppService appService, IServerNodeService serverNodeService)
        {
            _appService = appService;
            _configService = configService;
            _serverNodeService = serverNodeService;
        }

        public IActionResult Clients()
        {
            var report = WebsocketCollection.Instance.Report();

            return Json(new
            {
                success = true,
                data = report
            });
        }

        public IActionResult ServerNodeClients(string address)
        {
            var report = Program.RemoteServerNodeManager.GetClientsReport(address);

            return Json(new
            {
                success = true,
                data = report
            });
        }

        public async Task<IActionResult> AppCount()
        {
            var appCount = await _appService.CountEnabledAppsAsync();
            return Json(new
            {
                success = true,
                data = appCount
            });
        }

        public async Task<IActionResult> ConfigCount()
        {
            var configCount = await _configService.CountEnabledConfigsAsync();
            return Json(new
            {
                success = true,
                data = configCount
            });
        }

        public async Task<IActionResult> NodeCount()
        {
            var nodeCount = (await _serverNodeService.GetAllNodesAsync()).Count;
            return Json(new
            {
                success = true,
                data = nodeCount
            });
        }

        public async Task<IActionResult> RemoteNodesStatus()
        {
            var nodes = await _serverNodeService.GetAllNodesAsync();
            var result = new List<object>();

            nodes.ForEach(n => {
                result.Add(new { 
                    n,
                    server_status = Program.RemoteServerNodeManager.GetClientsReport(n.Address)
                });
            });

            return Json(new
            {
                success = true,
                data = result
            });
        }
    }
}
