using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api.v2;

/// <summary>
///     Version 2 service registry instance resources.
/// </summary>
[ApiController]
[Route("api/v2/service-instances")]
public sealed class ServiceInstancesController : ControllerBase
{
    private readonly IServiceInstanceManagementService _serviceInstanceManagementService;

    public ServiceInstancesController(
        IServiceInstanceManagementService serviceInstanceManagementService)
    {
        _serviceInstanceManagementService = serviceInstanceManagementService;
    }

    [HttpGet]
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Service_Read })]
    public async Task<ActionResult<IReadOnlyList<ServiceInstanceResource>>> GetAll([FromQuery] ServiceStatus? status)
    {
        var instances = await _serviceInstanceManagementService.GetAllAsync(status);

        return Ok(instances.Select(x => x.ToV2Resource()).ToList());
    }

    [HttpGet("{serviceInstanceId}", Name = RouteNames.ServiceInstance)]
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Service_Read })]
    public async Task<ActionResult<ServiceInstanceResource>> GetById(string serviceInstanceId)
    {
        var result = await _serviceInstanceManagementService.GetByIdAsync(serviceInstanceId);
        return result.Succeeded ? Ok(result.Value.ToV2Resource()) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<ServiceInstanceResource>> Register(RegisterServiceInstanceRequest request)
    {
        var result = await _serviceInstanceManagementService.RegisterAsync(new RegisterServiceInstanceCommand(
            request.ServiceId,
            request.Name,
            request.IpAddress,
            request.Port,
            JsonSerializer.Serialize(request.Metadata ?? []),
            request.HealthCheckUrl,
            request.AlarmUrl,
            request.HeartbeatMode,
            RegisterWay.Auto,
            RejectExisting: false));
        if (!result.Succeeded) return MapFailure(result.Error, "Service instance registration failed.");

        var resource = result.Value.Instance.ToV2Resource();
        if (result.Value.WasExisting) return Ok(resource);

        return CreatedAtRoute(RouteNames.ServiceInstance,
            new { serviceInstanceId = result.Value.UniqueId }, resource);
    }

    [HttpDelete("{serviceInstanceId}")]
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { Functions.Service_Delete })]
    public async Task<IActionResult> Unregister(string serviceInstanceId)
    {
        var result = await _serviceInstanceManagementService.UnregisterAsync(serviceInstanceId);
        return result.Succeeded ? NoContent() : MapFailure(result.Error, "Service instance could not be unregistered.");
    }

    [HttpPut("{serviceInstanceId}/heartbeat")]
    public async Task<ActionResult<HeartbeatResource>> Heartbeat(string serviceInstanceId)
    {
        var result = await _serviceInstanceManagementService.ReceiveHeartbeatAsync(serviceInstanceId);
        if (!result.Succeeded) return NotFound();

        return Ok(new HeartbeatResource
        {
            ServiceInstanceId = result.Value.ServiceInstanceId,
            ReceivedAt = result.Value.ReceivedAt,
            ServicesVersion = result.Value.ServicesVersion
        });
    }

    private ObjectResult MapFailure(ApplicationError error, string title)
    {
        return error switch
        {
            ApplicationError.NotFound => Problem(statusCode: 404, title: "Service instance not found."),
            ApplicationError.Conflict => Problem(statusCode: 409, title: "Service instance conflict."),
            ApplicationError.ValidationFailed => Problem(statusCode: 400, title: "Invalid service instance request."),
            ApplicationError.Forbidden => Problem(statusCode: 403, title: "Service instance operation forbidden."),
            _ => Problem(statusCode: 500, title: title)
        };
    }
}
