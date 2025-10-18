using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    /// <summary>
    /// Reports node status information, such as connected clients.
    /// </summary>
    public class ReportController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IAppService _appService;
        private readonly IServerNodeService _serverNodeService;
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IServiceInfoService _serviceInfoService;

        public ReportController(
            IConfigService configService, 
            IAppService appService, 
            IServerNodeService serverNodeService, 
            IRemoteServerNodeProxy remoteServerNodeProxy,
            IServiceInfoService serviceInfoService)
        {
            _appService = appService;
            _configService = configService;
            _serverNodeService = serverNodeService;
            _remoteServerNodeProxy = remoteServerNodeProxy;
            _serviceInfoService = serviceInfoService;
        }

        /// <summary>
        /// Get client connections on the current node.
        /// </summary>
        /// <returns></returns>
        public IActionResult Clients()
        {
            var report = WebsocketCollection.Instance.Report();

            return Json(report);
        }

        /// <summary>
        /// Get client connections on a specific node.
        /// </summary>
        /// <param name="address">Server address to inspect.</param>
        /// <returns>JSON result containing the client report.</returns>
        public IActionResult ServerNodeClients(string address)
        {
            var report = _remoteServerNodeProxy.GetClientsReportAsync(address);

            return Json(report);
        }

        public async Task<IActionResult> SearchServerNodeClients(string address, string appId, string env, int current, int pageSize)
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
            var nodes = await _serverNodeService.GetAllNodesAsync();
            if (string.IsNullOrEmpty(address))
            {
                addressess.AddRange(nodes.Where(x=>x.Status == NodeStatus.Online).Select(n => n.Id.ToString()));
            }
            else
            {
                if (nodes.Any(x=>x.Status == NodeStatus.Online && x.Id.ToString().Equals(address, StringComparison.CurrentCultureIgnoreCase)))
                {
                    addressess.Add(address);
                }
            }

            var clients = new List<ClientInfo>();
            foreach (var addr in addressess)
            {
                var report = await _remoteServerNodeProxy.GetClientsReportAsync(addr);
                if (report != null && report.Infos != null)
                {
                    clients.AddRange(report.Infos);
                }
            }

            // filter by env
            if (!string.IsNullOrEmpty(env))
            {
                clients = clients.Where(x => x.Env.Contains(env, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
            // filter by appid
            if (!string.IsNullOrEmpty(appId))
            {
                clients = clients.Where(x => x.AppId.Contains(appId, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

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
        /// Get the number of enabled applications.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AppCount()
        {
            var appCount = await _appService.CountEnabledAppsAsync();
            return Json(appCount);
        }

        /// <summary>
        /// Get the number of enabled configuration items.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ConfigCount()
        {
            var configCount = await _configService.CountEnabledConfigsAsync();
            return Json(configCount);
        }

        /// <summary>
        /// Get the number of server nodes.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> NodeCount()
        {
            var nodeCount = (await _serverNodeService.GetAllNodesAsync()).Count;
            return Json(nodeCount);
        }

        /// <summary>
        /// Get status information for all nodes.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> RemoteNodesStatus()
        {
            var nodes = await _serverNodeService.GetAllNodesAsync();
            var result = new List<object>();

            foreach (var serverNode in nodes)
            {
                if (serverNode.Status == NodeStatus.Offline)
                {
                    result.Add(new
                    {
                        n = serverNode,
                        server_status = new ClientInfos
                        { 
                            ClientCount = 0,
                            Infos = new List<ClientInfo>()
                        }
                    });
                    continue;
                }
                result.Add(new
                {
                    n = serverNode,
                    server_status = await _remoteServerNodeProxy.GetClientsReportAsync(serverNode.Id.ToString())
                });
            }

            return Json(result);
        }
        
        public async Task<IActionResult> ServiceCount()
        {
            var services = await _serviceInfoService.GetAllServiceInfoAsync();
            var serviceCount = services.Count;
            var serviceOnCount = services.Count(x => x.Status == ServiceStatus.Healthy);
            return Json(new
            {
                serviceCount,
                serviceOnCount
            });
        }
    }
}
