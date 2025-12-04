using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api;

/// <summary>
///     Node management API.
/// </summary>
[TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
[Route("api/[controller]")]
public class NodeController : Controller
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private readonly IServerNodeService _serverNodeService;
    private readonly ISysLogService _sysLogService;
    private readonly ITinyEventBus _tinyEventBus;

    public NodeController(IServerNodeService serverNodeService,
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

    /// <summary>
    ///     Get all nodes.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiNodeVM>>> GetAll()
    {
        var nodes = await _serverNodeService.GetAllNodesAsync();

        var vms = nodes.Select(x => x.ToApiNodeVM());

        return Json(vms);
    }

    /// <summary>
    ///     Create a node.
    /// </summary>
    /// <param name="model">Node payload.</param>
    /// <returns></returns>
    [ProducesResponseType(201)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Node_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ApiNodeVM model)
    {
        var requiredResult = CheckRequired(model);

        if (!requiredResult.Item1)
        {
            Response.StatusCode = 400;
            return Json(new
            {
                message = "Add node failed"
            });
        }

        var ctrl = new ServerNodeController(_serverNodeService, _sysLogService, _remoteServerNodeProxy, _tinyEventBus);
        ctrl.ControllerContext.HttpContext = HttpContext;
        var result = await ctrl.Add(model.ToServerNodeVM()) as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success == true) return Created("", "");

        Response.StatusCode = 400;
        return Json(new
        {
            obj?.message
        });
    }

    /// <summary>
    ///     Delete a node by address.
    /// </summary>
    /// <param name="address">Node address.</param>
    /// <returns></returns>
    [ProducesResponseType(204)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Node_Delete })]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string address)
    {
        var ctrl = new ServerNodeController(_serverNodeService, _sysLogService, _remoteServerNodeProxy, _tinyEventBus);
        ctrl.ControllerContext.HttpContext = HttpContext;
        var result = await ctrl.Delete(new ServerNodeVM { Address = address }) as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success == true) return NoContent();

        Response.StatusCode = 400;
        return Json(new
        {
            obj?.message
        });
    }

    private (bool, string) CheckRequired(ApiNodeVM model)
    {
        if (string.IsNullOrEmpty(model.Address)) return (false, "Address is required");

        return (true, "");
    }
}