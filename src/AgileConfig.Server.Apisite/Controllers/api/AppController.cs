using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Releases;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api;

/// <summary>
///     Application management API.
/// </summary>
[TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
[Route("api/[controller]")]
public class AppController : Controller
{
    private readonly IAppService _appService;
    private readonly IApplicationManagementService _applicationManagementService;
    private readonly IReleaseManagementService _releaseManagementService;

    public AppController(
        IAppService appService,
        IApplicationManagementService applicationManagementService,
        IReleaseManagementService releaseManagementService)
    {
        _appService = appService;
        _applicationManagementService = applicationManagementService;
        _releaseManagementService = releaseManagementService;
    }

    /// <summary>
    ///     Get all applications.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.App_Read })]
    public async Task<ActionResult<IEnumerable<ApiAppVM>>> GetAll()
    {
        var apps = await _appService.GetAllAppsAsync();
        var vms = apps.Select(x => x.ToApiAppVM());

        return Json(vms);
    }

    /// <summary>
    ///     Get an application by its identifier.
    /// </summary>
    /// <param name="id">Application ID.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.App_Read })]
    public async Task<ActionResult<ApiAppVM>> GetById(string id)
    {
        var app = await _appService.GetAsync(id);
        if (app == null)
        {
            Response.StatusCode = 404;
            return Json(new { message = Messages.AppNotFound });
        }

        var resource = app.ToApiAppVM();
        resource.InheritancedApps = (await _appService.GetInheritancedAppsAsync(id))
            .Select(x => x.Id)
            .ToList();
        return Json(resource);
    }

    /// <summary>
    ///     Create a new application.
    /// </summary>
    /// <param name="model">Application payload.</param>
    /// <returns></returns>
    [ProducesResponseType(201)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ApiAppVM model)
    {
        var requiredResult = CheckRequired(model);

        if (!requiredResult.Item1)
        {
            Response.StatusCode = 400;
            return Json(new
            {
                message = requiredResult.Item2
            });
        }

        var result = await _applicationManagementService.CreateAsync(new CreateApplicationCommand(
            model.Id,
            model.Name,
            model.Group,
            model.Secret,
            model.Enabled.GetValueOrDefault(),
            model.Inheritanced,
            null));
        if (result.Succeeded) return Created("/api/app/" + result.Value.Id, "");

        Response.StatusCode = 400;
        return Json(new { message = result.Error == ApplicationError.Conflict
            ? Messages.AppIdExists
            : Messages.CreateAppFailed });
    }

    /// <summary>
    ///     Update an existing application.
    /// </summary>
    /// <param name="id">Application ID.</param>
    /// <param name="model">Application payload.</param>
    /// <returns></returns>
    [ProducesResponseType(200)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.App_Edit })]
    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(string id, [FromBody] ApiAppVM model)
    {
        var requiredResult = CheckRequired(model);

        if (!requiredResult.Item1)
        {
            Response.StatusCode = 400;
            return Json(new
            {
                message = requiredResult.Item2
            });
        }

        model.Id = id;
        var result = await _applicationManagementService.UpdateAsync(new UpdateApplicationCommand(
            id,
            model.Name,
            model.Group,
            model.Secret,
            model.Enabled.GetValueOrDefault(),
            model.Inheritanced,
            null));
        if (result.Succeeded) return Ok();

        Response.StatusCode = result.Error == ApplicationError.NotFound ? 404 : 400;
        return Json(new { message = result.Error switch
        {
            ApplicationError.NotFound => Messages.AppNotFound,
            ApplicationError.ValidationFailed => Messages.DemoModeNoTestAppEdit,
            _ => Messages.UpdateAppFailed
        } });
    }

    /// <summary>
    ///     Delete an application.
    /// </summary>
    /// <param name="id">Application ID.</param>
    /// <returns></returns>
    [ProducesResponseType(204)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.App_Delete })]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _applicationManagementService.DeleteAsync(new DeleteApplicationCommand(id));
        if (result.Succeeded) return NoContent();

        Response.StatusCode = result.Error == ApplicationError.NotFound ? 404 : 400;
        return Json(new { message = result.Error == ApplicationError.NotFound
            ? (string)null
            : Messages.UpdateAppFailed });
    }

    /// <summary>
    ///     Publish pending configuration items of an application.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Config_Publish })]
    [HttpPost("publish")]
    public async Task<IActionResult> Publish(string appId, EnvString env)
    {
        var result = await _releaseManagementService.PublishAsync(
            new PublishConfigurationsCommand(appId, null, null, env.Value));
        if (result.Succeeded) return Ok();

        Response.StatusCode = result.Error == ApplicationError.NotFound ? 404 : 400;
        return Json(new { message = "上线配置失败，请查看错误日志" });
    }

    /// <summary>
    ///     Retrieve the publish history of an application.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.App_Read })]
    [HttpGet("Publish_History")]
    public async Task<ActionResult<IEnumerable<ApiPublishTimelineVM>>> PublishHistory(string appId, EnvString env)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        var history = await _releaseManagementService.GetAllAsync(appId, env.Value);
        var vms = history.Select(x => x.ToApiPublishTimelimeVM());

        return Json(vms);
    }

    /// <summary>
    ///     Roll back the application to the configuration at the specified publish history entry.
    /// </summary>
    /// <param name="historyId">Publish history identifier.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Config_Offline })]
    [HttpPost("rollback")]
    public async Task<IActionResult> Rollback(string historyId, EnvString env)
    {
        var result = await _releaseManagementService.RollbackAsync(
            new RollbackConfigurationCommand(historyId, env.Value));
        if (result.Succeeded) return Ok();

        Response.StatusCode = result.Error == ApplicationError.NotFound ? 404 : 400;
        return Json(new { message = "回滚失败，请查看错误日志。" });
    }

    private (bool, string) CheckRequired(ApiAppVM model)
    {
        if (string.IsNullOrEmpty(model.Id)) return (false, "Id is required");
        if (string.IsNullOrEmpty(model.Name)) return (false, "Name is required");

        return (true, "");
    }
}
