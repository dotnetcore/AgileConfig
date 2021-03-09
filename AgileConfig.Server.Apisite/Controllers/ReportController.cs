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
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;

        public ReportController(IConfigService configService, IAppService appService, IServerNodeService serverNodeService, IRemoteServerNodeProxy remoteServerNodeProxy)
        {
            _appService = appService;
            _configService = configService;
            _serverNodeService = serverNodeService;
            _remoteServerNodeProxy = remoteServerNodeProxy;
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
            var report = _remoteServerNodeProxy.GetClientsReport(address);

            return Json(report);
        }

        public async Task<IActionResult> SearchServerNodeClients(string address, int current, int pageSize)
        {
            if (current <= 0)
            {
                throw new ArgumentException("current can not less than 1 .");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("pageSize can not less than 1 .");
            }
            var addressess = new List<string>();
            if (string.IsNullOrEmpty(address))
            {
                var nodes = await _serverNodeService.GetAllNodesAsync();
                addressess.AddRange(nodes.Select(n => n.Address));
            }
            else
            {
                addressess.Add(address);
            }

            var clients = new List<ClientInfo>();
            addressess.ForEach(addr =>
            {
                var report = _remoteServerNodeProxy.GetClientsReport(addr);
                if (report != null && report.Infos != null)
                {
                    clients.AddRange(report.Infos);
                }
            });

            var page = clients.OrderBy(i => i.Address).ThenBy(i => i.Id).Skip((current - 1) * pageSize).Take(pageSize);

            return Json(new
            {
                current,
                pageSize,
                success = true,
                total = clients.Count,
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

            nodes.ForEach(n =>
            {
                result.Add(new
                {
                    n,
                    server_status = _remoteServerNodeProxy.GetClientsReport(n.Address)
                });
            });

            return Json(result);
        }
    }
}
