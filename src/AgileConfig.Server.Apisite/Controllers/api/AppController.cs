using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AgileConfig.Server.Apisite.Controllers.api;

/// <summary>
///     Application management API.
/// </summary>
[TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
[Route("api/[controller]")]
public class AppController : Controller
{
    private readonly Controllers.AppController _appController;
    private readonly IAppService _appService;
    private readonly Controllers.ConfigController _configController;
    private readonly IConfigService _configService;

    public AppController(IAppService appService,
        IConfigService configService,
        Controllers.AppController appController,
        Controllers.ConfigController configController)
    {
        _appService = appService;
        _configService = configService;

        _appController = appController;
        _configController = configController;
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
        var actionResult = await _appController.Get(id);
        var status = actionResult as IStatusCodeActionResult;

        var result = actionResult as JsonResult;
        dynamic obj = result?.Value;

        if (obj?.success ?? false)
        {
            AppVM appVM = obj.data;
            return Json(appVM.ToApiAppVM());
        }

        Response.StatusCode = status.StatusCode.Value;
        return Json(new
        {
            obj?.message
        });
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

        _appController.ControllerContext.HttpContext = HttpContext;

        var result = await _appController.Add(model.ToAppVM()) as JsonResult;

        dynamic obj = result?.Value;

        if (obj?.success == true) return Created("/api/app/" + obj.data.Id, "");

        Response.StatusCode = 400;
        return Json(new
        {
            obj?.message
        });
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

        _appController.ControllerContext.HttpContext = HttpContext;

        model.Id = id;
        var actionResult = await _appController.Edit(model.ToAppVM());
        var status = actionResult as IStatusCodeActionResult;
        var result = actionResult as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success ?? false) return Ok();

        Response.StatusCode = status.StatusCode.Value;
        return Json(new
        {
            obj?.message
        });
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
        _appController.ControllerContext.HttpContext = HttpContext;

        var actionResult = await _appController.Delete(id);
        var status = actionResult as IStatusCodeActionResult;
        var result = actionResult as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success ?? false) return NoContent();

        Response.StatusCode = status.StatusCode.Value;
        return Json(new
        {
            obj?.message
        });
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
        _configController.ControllerContext.HttpContext = HttpContext;

        var actionResult = await _configController.Publish(new PublishLogVM
        {
            AppId = appId
        }, env);
        var status = actionResult as IStatusCodeActionResult;
        var result = actionResult as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success ?? false) return Ok();

        Response.StatusCode = status.StatusCode.Value;
        return Json(new
        {
            obj?.message
        });
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

        var history = await _configService.GetPublishTimelineHistoryAsync(appId, env.Value);

        history = history.OrderByDescending(x => x.Version).ToList();

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
        _configController.ControllerContext.HttpContext = HttpContext;

        var actionResult = await _configController.Rollback(historyId, env);
        var status = actionResult as IStatusCodeActionResult;
        var result = actionResult as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success ?? false) return Ok();

        Response.StatusCode = status.StatusCode.Value;
        return Json(new
        {
            obj?.message
        });
    }

    private (bool, string) CheckRequired(ApiAppVM model)
    {
        if (string.IsNullOrEmpty(model.Id)) return (false, "Id is required");
        if (string.IsNullOrEmpty(model.Name)) return (false, "Name is required");

        return (true, "");
    }
}