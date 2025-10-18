using Agile.Config.Protocol;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Dynamic;
using System.Threading.Tasks;
using AgileConfig.Server.Common.Resources;

namespace AgileConfig.Server.Apisite.Controllers
{
    /// <summary>
    /// Handles console web requests that proxy operations to remote server nodes (distinct from RemoteOpController).
    /// </summary>
    [Authorize]
    public class RemoteServerProxyController : Controller
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly ITinyEventBus _tinyEventBus;
        private readonly ILogger _logger;

        public RemoteServerProxyController(
            IRemoteServerNodeProxy remoteServerNodeProxy,
            ILoggerFactory loggerFactory,
            ITinyEventBus tinyEventBus
        )
        {
            _remoteServerNodeProxy = remoteServerNodeProxy;
            _tinyEventBus = tinyEventBus;
            _logger = loggerFactory.CreateLogger<RemoteServerProxyController>();
        }

        /// <summary>
        /// Notify a node to disconnect a specific client.
        /// </summary>
        /// <param name="address">Remote node address.</param>
        /// <param name="clientId">Client identifier to disconnect.</param>
        /// <returns>Operation result.</returns>
        [HttpPost]
        public async Task<IActionResult> Client_Offline(string address, string clientId)
        {
            if (Appsettings.IsPreviewMode)
            {
                return Json(new
                {
                    success = false,
                    message = Messages.DemoModeNoClientDisconnect
                });
            }

            var action = new WebsocketAction { Action = ActionConst.Offline, Module = ActionModule.ConfigCenter };
            var result = await _remoteServerNodeProxy.OneClientDoActionAsync(address, clientId, action);
            if (result)
            {
                _tinyEventBus.Fire(new DiscoinnectSuccessful(clientId, this.GetCurrentUserName()));
            }

            _logger.LogInformation("Request remote node {0} 's action OneClientDoAction {1} .", address,
                result ? "success" : "fail");

            return Json(new
            {
                success = true,
            });
        }

        /// <summary>
        /// Notify a node to instruct all clients to reload configuration.
        /// </summary>
        /// <param name="address">Remote node address.</param>
        /// <returns>Operation result.</returns>
        [HttpPost]
        public async Task<IActionResult> AllClients_Reload(string address)
        {
            var action = new WebsocketAction { Action = ActionConst.Reload, Module = ActionModule.ConfigCenter };
            var result = await _remoteServerNodeProxy.AllClientsDoActionAsync(address, action);

            _logger.LogInformation("Request remote node {0} 's action AllClientsDoAction {1} .", address,
                result ? "success" : "fail");

            return Json(new
            {
                success = true,
            });
        }

        /// <summary>
        /// Notify a node to instruct a single client to reload configuration.
        /// </summary>
        /// <param name="address">Remote node address.</param>
        /// <param name="clientId">Client identifier to reload.</param>
        /// <returns>Operation result.</returns>
        [HttpPost]
        public async Task<IActionResult> Client_Reload(string address, string clientId)
        {
            var action = new WebsocketAction { Action = ActionConst.Reload, Module = ActionModule.ConfigCenter };
            var result = await _remoteServerNodeProxy.OneClientDoActionAsync(address, clientId, action);

            _logger.LogInformation("Request remote node {0} 's action OneClientDoAction {1} .", address,
                result ? "success" : "fail");

            return Json(new
            {
                success = true,
            });
        }
    }
}