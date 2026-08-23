using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api.v2;

/// <summary>
///     Version 2 application resources.
/// </summary>
[ApiController]
[Route("api/v2/applications")]
[TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
public sealed class ApplicationsController : ControllerBase
{
    private readonly IApplicationManagementService _applicationManagementService;

    public ApplicationsController(IApplicationManagementService applicationManagementService)
    {
        _applicationManagementService = applicationManagementService;
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Read })]
    public async Task<ActionResult<IReadOnlyList<ApplicationResource>>> GetAll()
    {
        var applications = await _applicationManagementService.GetAllAsync();
        var resources = applications
            .Select(x => x.Application.ToV2Resource(x.InheritedApplicationIds))
            .ToList();
        return Ok(resources);
    }

    [HttpGet("{applicationId}", Name = RouteNames.Application)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Read })]
    public async Task<ActionResult<ApplicationResource>> GetById(string applicationId)
    {
        var application = await _applicationManagementService.GetAsync(applicationId);
        return application == null
            ? NotFound()
            : Ok(application.Application.ToV2Resource(application.InheritedApplicationIds));
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Add })]
    public async Task<ActionResult<ApplicationResource>> Create(CreateApplicationRequest request)
    {
        var result = await _applicationManagementService.CreateAsync(new CreateApplicationCommand(
            request.Id,
            request.Name,
            request.Group,
            request.Secret,
            request.Enabled,
            request.IsInheritanceSource,
            request.InheritsFrom));
        if (!result.Succeeded)
            return ToFailure(
                result,
                "Application creation failed.",
                "An application with the same identifier already exists.",
                "The application could not be created.");

        var application = await _applicationManagementService.GetAsync(result.Value.Id);
        if (application == null) return Problem(title: "The created application could not be loaded.");

        var resource = application.Application.ToV2Resource(application.InheritedApplicationIds);
        return CreatedAtRoute(RouteNames.Application, new { applicationId = resource.Id }, resource);
    }

    [HttpPut("{applicationId}")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Edit })]
    public async Task<ActionResult<ApplicationResource>> Update(string applicationId, UpdateApplicationRequest request)
    {
        var result = await _applicationManagementService.UpdateAsync(new UpdateApplicationCommand(
            applicationId,
            request.Name,
            request.Group,
            request.Secret,
            request.Enabled,
            request.IsInheritanceSource,
            request.InheritsFrom));
        if (!result.Succeeded)
            return ToFailure(
                result,
                "Application update failed.",
                operationDetail: "The application could not be updated.");

        var application = await _applicationManagementService.GetAsync(result.Value.Id);
        return application == null
            ? Problem(title: "The updated application could not be loaded.")
            : Ok(application.Application.ToV2Resource(application.InheritedApplicationIds));
    }

    [HttpDelete("{applicationId}")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Delete })]
    public async Task<IActionResult> Delete(string applicationId)
    {
        var result = await _applicationManagementService.DeleteAsync(new DeleteApplicationCommand(applicationId));
        return result.Succeeded
            ? NoContent()
            : ToFailure(
                result,
                "Application deletion failed.",
                operationDetail: "The application could not be deleted.");
    }

    private ActionResult ToFailure(
        ApplicationResult<App> result,
        string title,
        string conflictDetail = null,
        string operationDetail = null)
    {
        var detail = result.Error switch
        {
            ApplicationError.Conflict => conflictDetail ??
                "The application could not be updated because it conflicts with another resource.",
            ApplicationError.ValidationFailed => "The application cannot be modified in preview mode.",
            ApplicationError.OperationFailed => operationDetail,
            _ => null
        };
        var statusCode = result.Error switch
        {
            ApplicationError.NotFound => 404,
            ApplicationError.Conflict => 409,
            ApplicationError.ValidationFailed => 400,
            ApplicationError.Forbidden => 403,
            _ => 500
        };
        return Problem(statusCode: statusCode, title: title, detail: detail);
    }

}

internal static class RouteNames
{
    public const string Application = "V2_GetApplication";
    public const string Configuration = "V2_GetConfiguration";
    public const string Release = "V2_GetRelease";
    public const string Node = "V2_GetNode";
    public const string ServiceInstance = "V2_GetServiceInstance";
}
