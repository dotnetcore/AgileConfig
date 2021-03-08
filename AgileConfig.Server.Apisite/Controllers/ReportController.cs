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
    /// <summary>
    /// 这个Controller用来上报节点的一些情况，比如链接了多少客户端等信息
    /// </summary>
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

        /// <summary>
        /// 获取本节点的客户端信息
        /// </summary>
        /// <returns></returns>
        public IActionResult Clients()
        {
            var report = WebsocketCollection.Instance.Report();

            return Json(report);
        }

        /// <summary>
        /// 获取某个节点的客户端信息
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IActionResult ServerNodeClients(string address)
        {
            var report = Program.RemoteServerNodeProxy.GetClientsReport(address);

            return Json(report);
        }

        public IActionResult SearchServerNodeClients(string address,int current, int pageSize)
        {
            if (current <= 0)
            {
                throw new ArgumentException("current can not less than 1 .");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("pageSize can not less than 1 .");
            }
            if (string.IsNullOrEmpty(address))
            {
                return Json(new
                {
                    current,
                    pageSize,
                    success = true,
                    total = 0,
                    data = new List<object>()
                }) ;
            }

            var report = Program.RemoteServerNodeProxy.GetClientsReport(address);
            if (report == null || report.Infos == null)
            {
                return  Json(new
                {
                    current,
                    pageSize,
                    success = true,
                    total = 0,
                    data = new List<object>()
                });
            }
            var page = report.Infos.OrderBy(i=>i.Id).Skip((current-1)*pageSize).Take(pageSize);

            return Json(new
            {
                current,
                pageSize,
                success = true,
                total = report.Infos.Count,
                data = page
            });
        }

        /// <summary>
        /// 获取App数量
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AppCount()
        {
            var appCount = await _appService.CountEnabledAppsAsync();
            return Json(appCount);
        }

        /// <summary>
        /// 获取Config项目数量
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ConfigCount()
        {
            var configCount = await _configService.CountEnabledConfigsAsync();
            return Json(configCount);
        }

        /// <summary>
        /// 获取Config项目数量
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> NodeCount()
        {
            var nodeCount = (await _serverNodeService.GetAllNodesAsync()).Count;
            return Json(nodeCount);
        }

        /// <summary>
        /// 获取所有节点的状态信息
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RemoteNodesStatus()
        {
            var nodes = await _serverNodeService.GetAllNodesAsync();
            var result = new List<object>();

            nodes.ForEach(n => {
                result.Add(new { 
                    n,
                    server_status = Program.RemoteServerNodeProxy.GetClientsReport(n.Address)
                });
            });

            return Json(result);
        }
    }
}
