using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Common.Resources;
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
    private readonly INodeManagementService _nodeManagementService;

    public NodeController(INodeManagementService nodeManagementService)
    {
        _nodeManagementService = nodeManagementService;
    }

    /// <summary>
    ///     Get all nodes.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiNodeVM>>> GetAll()
    {
        var nodes = await _nodeManagementService.GetAllAsync();

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

        var result = await _nodeManagementService.CreateAsync(new CreateNodeCommand(model.Address, model.Remark));
        if (result.Succeeded) return Created("", "");

        Response.StatusCode = 400;
        return Json(new
        {
            message = GetCreateErrorMessage(result.Error)
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
        var result = await _nodeManagementService.DeleteAsync(address);
        if (result.Succeeded) return NoContent();

        Response.StatusCode = 400;
        return Json(new
        {
            message = GetDeleteErrorMessage(result.Error)
        });
    }

    private static (bool, string) CheckRequired(ApiNodeVM model)
    {
        if (string.IsNullOrEmpty(model.Address)) return (false, "Address is required");

        return (true, "");
    }

    private static string GetCreateErrorMessage(ApplicationError error)
    {
        return error == ApplicationError.Conflict ? Messages.NodeAlreadyExists : Messages.AddNodeFailed;
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
