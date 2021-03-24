using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers
{
    /// <summary>
    /// 这个Controller是控制台网页跟后台的接口，不要跟RemoteOp那个Controller混淆
    /// </summary>
    [Authorize]
    public class RemoteServerProxyController : Controller
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly ILogger _logger;

        public RemoteServerProxyController(
            IServerNodeService serverNodeService,
            IRemoteServerNodeProxy remoteServerNodeProxy,
            ILoggerFactory loggerFactory,
            ISysLogService sysLogService)
        {
            _remoteServerNodeProxy = remoteServerNodeProxy;
            _logger = loggerFactory.CreateLogger<RemoteServerProxyController>();
        }

        /// <summary>
        /// 通知一个节点的某个客户端离线
        /// </summary>
        /// <param name="address"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Client_Offline(string address, string clientId)
        {
            if (Appsettings.IsPreviewMode)
            {
                return Json(new
                {
                    success = false,
                    message = "演示模式请勿断开客户端"
                });
            }

            var action = new WebsocketAction { Action = "offline" };
            var result = await _remoteServerNodeProxy.OneClientDoActionAsync(address, clientId, action);

            _logger.LogInformation("Request remote node {0} 's action OneClientDoAction {1} .", address, result ? "success" : "fail");

            return Json(new
            {
                success = true,
            });
        }
        /// <summary>
        /// 通知某个节点让所有的客户端刷新配置项
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AllClients_Reload(string address)
        {
            var action = new WebsocketAction { Action = "reload" };
            var result = await _remoteServerNodeProxy.AllClientsDoActionAsync(address, action);

            _logger.LogInformation("Request remote node {0} 's action AllClientsDoAction {1} .", address, result ? "success" : "fail");

            return Json(new
            {
                success = true,
            });
        }

        /// <summary>
        /// 通知某个节点个某个客户端刷新配置项
        /// </summary>
        /// <param name="address"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Client_Reload(string address, string clientId)
        {
            var action = new WebsocketAction { Action = "reload" };
            var result = await _remoteServerNodeProxy.OneClientDoActionAsync(address, clientId, action);

            _logger.LogInformation("Request remote node {0} 's action OneClientDoAction {1} .", address, result ? "success" : "fail");

            return Json(new
            {
                success = true,
            });
        }
    }
}
