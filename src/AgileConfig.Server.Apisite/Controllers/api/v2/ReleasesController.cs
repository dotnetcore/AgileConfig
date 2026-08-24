using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Releases;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api.v2;

/// <summary>
///     Version 2 configuration release and rollback resources.
/// </summary>
[ApiController]
[Route("api/v2/applications/{applicationId}/environments/{environment}/releases")]
[TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
[TypeFilter(typeof(V2EnvironmentFilterAttribute))]
public sealed class ReleasesController : ControllerBase
{
    private readonly IApplicationManagementService _applicationManagementService;
    private readonly IReleaseManagementService _releaseManagementService;

    public ReleasesController(
        IApplicationManagementService applicationManagementService,
        IReleaseManagementService releaseManagementService)
    {
        _applicationManagementService = applicationManagementService;
        _releaseManagementService = releaseManagementService;
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Read })]
    public async Task<ActionResult<IReadOnlyList<ReleaseResource>>> GetAll(string applicationId, string environment)
    {
        if (await _applicationManagementService.GetAsync(applicationId) == null) return NotFound();

        var releases = await _releaseManagementService.GetAllAsync(applicationId, environment);
        return Ok(releases.Select(x => x.ToV2Resource()).ToList());
    }

    [HttpGet("{releaseId}", Name = RouteNames.Release)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Read })]
    public async Task<ActionResult<ReleaseResource>> GetById(string applicationId, string environment, string releaseId)
    {
        var release = await FindRelease(applicationId, environment, releaseId);
        return release == null ? NotFound() : Ok(release);
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Config_Publish })]
    public async Task<ActionResult<ReleaseResource>> Create(
        string applicationId,
        string environment,
        CreateReleaseRequest request)
    {
        var result = await _releaseManagementService.PublishAsync(new PublishConfigurationsCommand(
            applicationId,
            request.ConfigurationIds?.ToArray(),
            request.Log,
            environment));
        if (!result.Succeeded) return MapFailure(result.Error, "Configuration release failed.");

        var release = result.Value;

        return CreatedAtRoute(RouteNames.Release, new { applicationId, environment, releaseId = release.Id },
            release.ToV2Resource());
    }

    [HttpPost("rollbacks")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Config_Offline })]
    public async Task<IActionResult> Rollback(
        string applicationId,
        string environment,
        CreateRollbackRequest request)
    {
        if (await FindRelease(applicationId, environment, request.ReleaseId) == null) return NotFound();

        var result = await _releaseManagementService.RollbackAsync(
            new RollbackConfigurationCommand(request.ReleaseId, environment));
        return result.Succeeded ? NoContent() : MapFailure(result.Error, "Configuration rollback failed.");
    }

    private async Task<ReleaseResource> FindRelease(string applicationId, string environment, string releaseId)
    {
        var release = await _releaseManagementService.GetAsync(applicationId, environment, releaseId);
        return release?.ToV2Resource();
    }

    private ObjectResult MapFailure(ApplicationError error, string title)
    {
        return error switch
        {
            ApplicationError.NotFound => Problem(statusCode: 404, title: "Configuration release not found."),
            ApplicationError.Conflict => Problem(statusCode: 409, title: "Configuration release conflict."),
            ApplicationError.ValidationFailed => Problem(statusCode: 400, title: "Invalid release request."),
            ApplicationError.Forbidden => Problem(statusCode: 403, title: "Configuration release forbidden."),
            _ => Problem(statusCode: 500, title: title)
        };
    }
}
