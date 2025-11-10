using System;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
[ModelVaildate]
public class ServerNodeController : Controller
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private readonly IServerNodeService _serverNodeService;
    private readonly ISysLogService _sysLogService;
    private readonly ITinyEventBus _tinyEventBus;

    public ServerNodeController(IServerNodeService serverNodeService,
        ISysLogService sysLogService,
        IRemoteServerNodeProxy remoteServerNodeProxy,
        ITinyEventBus tinyEventBus
    )
    {
        _serverNodeService = serverNodeService;
        _sysLogService = sysLogService;
        _remoteServerNodeProxy = remoteServerNodeProxy;
        _tinyEventBus = tinyEventBus;
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Node.Add", Functions.Node_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ServerNodeVM model)
    {
        if (model == null) throw new ArgumentNullException("model");
        model.Address = model.Address.TrimEnd('/');
        var oldNode = await _serverNodeService.GetAsync(model.Address);
        if (oldNode != null)
            return Json(new
            {
                success = false,
                message = Messages.NodeAlreadyExists
            });

        var node = new ServerNode();
        node.Id = model.Address.TrimEnd('/');
        node.Remark = model.Remark;
        node.Status = NodeStatus.Offline;
        node.CreateTime = DateTime.Now;

        var result = await _serverNodeService.AddAsync(node);
        if (result)
        {
            _tinyEventBus.Fire(new AddNodeSuccessful(node, this.GetCurrentUserName()));
            await _remoteServerNodeProxy.TestEchoAsync(node.Id);
        }

        return Json(new
        {
            data = node,
            success = result,
            message = !result ? Messages.AddNodeFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Node.Delete", Functions.Node_Delete })]
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] ServerNodeVM model)
    {
        if (Appsettings.IsPreviewMode)
            return Json(new
            {
                success = false,
                message = Messages.DemoModeNoNodeDelete
            });
        if (model == null) throw new ArgumentNullException("model");

        var node = await _serverNodeService.GetAsync(model.Address);
        if (node == null)
            return Json(new
            {
                success = false,
                message = Messages.NodeNotFound
            });

        var result = await _serverNodeService.DeleteAsync(node);
        if (result) _tinyEventBus.Fire(new DeleteNodeSuccessful(node, this.GetCurrentUserName()));
        return Json(new
        {
            success = result,
            message = !result ? Messages.DeleteNodeFailed : ""
        });
    }

    [HttpGet]
    public async Task<IActionResult> All()
    {
        var nodes = await _serverNodeService.GetAllNodesAsync();

        var vms = nodes.OrderBy(x => x.CreateTime).Select(x =>
        {
            return new ServerNodeVM
            {
                Address = x.Id,
                Remark = x.Remark,
                LastEchoTime = x.LastEchoTime,
                Status = x.Status
            };
        });

        return Json(new
        {
            success = true,
            data = vms
        });
    }
}