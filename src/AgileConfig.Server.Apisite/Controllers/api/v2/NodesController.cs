using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api.v2;

/// <summary>
///     Version 2 cluster node resources.
/// </summary>
[ApiController]
[Route("api/v2/nodes")]
[TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
public sealed class NodesController : ControllerBase
{
    private readonly INodeManagementService _nodeManagementService;

    public NodesController(INodeManagementService nodeManagementService)
    {
        _nodeManagementService = nodeManagementService;
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Node_Read })]
    public async Task<ActionResult<IReadOnlyList<NodeResource>>> GetAll()
    {
        var nodes = await _nodeManagementService.GetAllAsync();
        return Ok(nodes.Select(x => x.ToV2Resource()).ToList());
    }

    [HttpGet("{nodeId}", Name = RouteNames.Node)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Node_Read })]
    public async Task<ActionResult<NodeResource>> GetById(string nodeId)
    {
        var node = await FindNode(nodeId);
        return node == null ? NotFound() : Ok(node.ToV2Resource());
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Node_Add })]
    public async Task<ActionResult<NodeResource>> Create(CreateNodeRequest request)
    {
        var result = await _nodeManagementService.CreateAsync(new CreateNodeCommand(request.Address, request.Remark));
        if (!result.Succeeded) return MapFailure(result.Error, "Node creation failed.");

        var resource = result.Value.ToV2Resource();
        return CreatedAtRoute(RouteNames.Node, new { nodeId = resource.Id }, resource);
    }

    [HttpDelete("{nodeId}")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Node_Delete })]
    public async Task<IActionResult> Delete(string nodeId)
    {
        var address = V2MappingExtensions.DecodeNodeId(nodeId);
        if (address == null) return NotFound();

        var result = await _nodeManagementService.DeleteAsync(address);
        return result.Succeeded ? NoContent() : MapFailure(result.Error, "Node deletion failed.");
    }

    private async Task<AgileConfig.Server.Data.Entity.ServerNode> FindNode(string nodeId)
    {
        var address = V2MappingExtensions.DecodeNodeId(nodeId);
        return address == null ? null : await _nodeManagementService.GetByAddressAsync(address);
    }

    private ObjectResult MapFailure(ApplicationError error, string title)
    {
        return error switch
        {
            ApplicationError.Conflict => Problem(statusCode: 409, title: "Node conflict.",
                detail: "A node with the same address already exists."),
            ApplicationError.NotFound => Problem(statusCode: 404, title: "Node not found."),
            ApplicationError.ValidationFailed => Problem(statusCode: 400, title: "Invalid node request."),
            ApplicationError.Forbidden => Problem(statusCode: 403, title: "Node operation forbidden."),
            _ => Problem(statusCode: 500, title: title)
        };
    }
}
