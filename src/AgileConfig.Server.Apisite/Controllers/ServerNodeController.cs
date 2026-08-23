using System;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
[ModelVaildate]
public class ServerNodeController : Controller
{
    private readonly INodeManagementService _nodeManagementService;

    public ServerNodeController(INodeManagementService nodeManagementService)
    {
        _nodeManagementService = nodeManagementService;
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Node_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ServerNodeVM model)
    {
        if (model == null) throw new ArgumentNullException("model");

        var result = await _nodeManagementService.CreateAsync(new CreateNodeCommand(model.Address, model.Remark));
        if (!result.Succeeded && result.Error == ApplicationError.Conflict)
            return Json(new
            {
                success = false,
                message = Messages.NodeAlreadyExists
            });

        return Json(new
        {
            data = result.Value,
            success = result.Succeeded,
            message = !result.Succeeded ? Messages.AddNodeFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Node_Delete })]
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] ServerNodeVM model)
    {
        if (model == null) throw new ArgumentNullException("model");

        var result = await _nodeManagementService.DeleteAsync(model.Address);
        return Json(new
        {
            success = result.Succeeded,
            message = !result.Succeeded ? GetDeleteErrorMessage(result.Error) : ""
        });
    }

    [HttpGet]
    public async Task<IActionResult> All()
    {
        var nodes = await _nodeManagementService.GetAllAsync();

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

    private static string GetDeleteErrorMessage(ApplicationError error)
    {
        return error switch
        {
            ApplicationError.Forbidden => Messages.DemoModeNoNodeDelete,
            ApplicationError.NotFound => Messages.NodeNotFound,
            _ => Messages.DeleteNodeFailed
        };
    }
}
