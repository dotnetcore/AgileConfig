using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite.Controllers;

/// <summary>
///     Handles console web requests that proxy operations to remote server nodes (distinct from RemoteOpController).
/// </summary>
[Authorize]
public class RemoteServerProxyController : Controller
{
    private readonly ILogger _logger;
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private readonly IServerNodeService _serverNodeService;
    private readonly ITinyEventBus _tinyEventBus;

    public RemoteServerProxyController(
        IRemoteServerNodeProxy remoteServerNodeProxy,
        ILoggerFactory loggerFactory,
        IServerNodeService serverNodeService,
        ITinyEventBus tinyEventBus
    )
    {
        _serverNodeService = serverNodeService;
        _remoteServerNodeProxy = remoteServerNodeProxy;
        _tinyEventBus = tinyEventBus;
        _logger = loggerFactory.CreateLogger<RemoteServerProxyController>();
    }

    /// <summary>
    ///     Notify a node to disconnect a specific client.
    /// </summary>
    /// <param name="address">Remote node address.</param>
    /// <param name="clientId">Client identifier to disconnect.</param>
    /// <returns>Operation result.</returns>
    [HttpPost]
    public async Task<IActionResult> Client_Offline(string address, string clientId)
    {
        if (Appsettings.IsPreviewMode)
            return Json(new
            {
                success = false,
                message = Messages.DemoModeNoClientDisconnect
            });

        var action = new WebsocketAction { Action = ActionConst.Offline, Module = ActionModule.ConfigCenter };
        var result = await _remoteServerNodeProxy.OneClientDoActionAsync(address, clientId, action);
        if (result) _tinyEventBus.Fire(new DiscoinnectSuccessful(clientId, this.GetCurrentUserName()));

        _logger.LogInformation("Request remote node {0} 's action OneClientDoAction {1} .", address,
            result ? "success" : "fail");

        return Json(new
        {
            success = true
        });
    }

    /// <summary>
    ///     Notify a node to instruct all clients to reload configuration.
    /// </summary>
    /// <param name="address">Remote node address.</param>
    /// <returns>Operation result.</returns>
    [HttpPost]
    public async Task<IActionResult> AllClients_Reload()
    {
        var nodes = await _serverNodeService.GetAllNodesAsync();
        var action = new WebsocketAction { Action = ActionConst.Reload, Module = ActionModule.ConfigCenter };
        foreach (var node in nodes)
            if (node.Status == NodeStatus.Online)
            {
                var result = await _remoteServerNodeProxy.AllClientsDoActionAsync(node.Id, action);
                _logger.LogInformation("Request remote node {0} 's action AllClientsDoAction {1} .", node.Id,
                    result ? "success" : "fail");
            }

        return Json(new
        {
            success = true
        });
    }

    /// <summary>
    ///     Notify a node to instruct a single client to reload configuration.
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
            success = true
        });
    }
}