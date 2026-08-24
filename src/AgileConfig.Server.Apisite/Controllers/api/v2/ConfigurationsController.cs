using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api.v2;

/// <summary>
///     Version 2 application configuration resources.
/// </summary>
[ApiController]
[Route("api/v2/applications/{applicationId}/environments/{environment}/configurations")]
[TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
[TypeFilter(typeof(V2EnvironmentFilterAttribute))]
public sealed class ConfigurationsController : ControllerBase
{
    private readonly IApplicationManagementService _applicationManagementService;
    private readonly IConfigurationManagementService _configurationManagementService;

    public ConfigurationsController(
        IApplicationManagementService applicationManagementService,
        IConfigurationManagementService configurationManagementService)
    {
        _applicationManagementService = applicationManagementService;
        _configurationManagementService = configurationManagementService;
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Config_Read })]
    public async Task<ActionResult<IReadOnlyList<ConfigurationResource>>> GetAll(
        string applicationId,
        string environment)
    {
        if (await _applicationManagementService.GetAsync(applicationId) == null) return NotFound();

        var configurations = await _configurationManagementService.GetAllAsync(applicationId, environment);
        return Ok(configurations.Select(x => x.ToV2Resource()).ToList());
    }

    [HttpGet("{configurationId}", Name = RouteNames.Configuration)]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Config_Read })]
    public async Task<ActionResult<ConfigurationResource>> GetById(
        string applicationId,
        string environment,
        string configurationId)
    {
        var configuration = await FindConfiguration(applicationId, environment, configurationId);
        return configuration == null ? NotFound() : Ok(configuration.ToV2Resource());
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Config_Add })]
    public async Task<ActionResult<ConfigurationResource>> Create(
        string applicationId,
        string environment,
        CreateConfigurationRequest request)
    {
        var result = await _configurationManagementService.CreateAsync(new CreateConfigurationCommand(
            null,
            applicationId,
            request.Group,
            request.Key,
            request.Value,
            request.Description,
            environment));
        if (!result.Succeeded) return MapFailure(result.Error, "Configuration creation failed.");

        var resource = result.Value;

        return CreatedAtRoute(RouteNames.Configuration, new
        {
            applicationId,
            environment,
            configurationId = resource.Id
        }, resource.ToV2Resource());
    }

    [HttpPut("{configurationId}")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Config_Edit })]
    public async Task<ActionResult<ConfigurationResource>> Update(
        string applicationId,
        string environment,
        string configurationId,
        UpdateConfigurationRequest request)
    {
        var existing = await FindConfiguration(applicationId, environment, configurationId);
        if (existing == null) return NotFound();

        var result = await _configurationManagementService.UpdateAsync(new UpdateConfigurationCommand(
            configurationId,
            applicationId,
            request.Group,
            request.Key,
            request.Value,
            request.Description,
            environment));
        if (!result.Succeeded) return MapFailure(result.Error, "Configuration update failed.");

        return Ok(result.Value.ToV2Resource());
    }

    [HttpDelete("{configurationId}")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Config_Delete })]
    public async Task<IActionResult> Delete(string applicationId, string environment, string configurationId)
    {
        if (await FindConfiguration(applicationId, environment, configurationId) == null) return NotFound();

        var result = await _configurationManagementService.DeleteAsync(
            new DeleteConfigurationCommand(configurationId, environment));
        return result.Succeeded ? NoContent() : MapFailure(result.Error, "Configuration deletion failed.");
    }

    private async Task<Config> FindConfiguration(string applicationId, string environment, string configurationId)
    {
        return await _configurationManagementService.GetAsync(applicationId, environment, configurationId);
    }

    private ObjectResult MapFailure(ApplicationError error, string title)
    {
        return error switch
        {
            ApplicationError.NotFound => Problem(statusCode: 404, title: "Configuration not found."),
            ApplicationError.Conflict => Problem(statusCode: 409, title: "Configuration conflict.",
                detail: "A configuration with the same group and key already exists."),
            ApplicationError.ValidationFailed => Problem(statusCode: 400, title: "Invalid configuration request."),
            ApplicationError.Forbidden => Problem(statusCode: 403, title: "Configuration operation forbidden."),
            _ => Problem(statusCode: 500, title: title)
        };
    }
}
