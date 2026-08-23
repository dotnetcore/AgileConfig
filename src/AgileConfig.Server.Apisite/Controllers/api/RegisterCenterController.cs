using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Application;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers.api;

/// <summary>
///     Service registration center API.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RegisterCenterController : Controller
{
    private readonly IServiceInstanceManagementService _serviceInstanceManagementService;
    private readonly IServiceInfoService _serviceInfoService;

    public RegisterCenterController(
        IServiceInstanceManagementService serviceInstanceManagementService,
        IServiceInfoService serviceInfoService
    )
    {
        _serviceInstanceManagementService = serviceInstanceManagementService;
        _serviceInfoService = serviceInfoService;
    }

    [HttpPost]
    public async Task<RegisterResultVM> Register([FromBody] RegisterServiceInfoVM model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var result = await _serviceInstanceManagementService.RegisterAsync(new RegisterServiceInstanceCommand(
            model.ServiceId,
            model.ServiceName,
            model.Ip,
            model.Port,
            JsonSerializer.Serialize(model.MetaData ?? []),
            model.CheckUrl,
            model.AlarmUrl,
            model.HeartBeatMode,
            RegisterWay.Auto,
            RejectExisting: false,
            AcceptMissingIdentifier: true));

        return new RegisterResultVM
        {
            UniqueId = result.Succeeded ? result.Value.UniqueId : null
        };
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult<RegisterResultVM>> UnRegister(string id, [FromBody] RegisterServiceInfoVM vm)
    {
        var result = await _serviceInstanceManagementService.UnregisterAsync(id, vm?.ServiceId);
        if (!result.Succeeded && result.Error == ApplicationError.NotFound) return NotFound();

        return new RegisterResultVM
        {
            UniqueId = id
        };
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult<HeartbeatResultVM>> Heartbeat([FromBody] HeartbeatParam param)
    {
        ArgumentNullException.ThrowIfNull(param);

        var result = await _serviceInstanceManagementService.ReceiveHeartbeatAsync(param.UniqueId);

        if (result.Succeeded)
        {
            return new HeartbeatResultVM
            {
                Action = ActionConst.Ping,
                Data = result.Value.ServicesVersion,
                Module = ActionModule.RegisterCenter
            };
        }

        return NotFound();
    }

    [HttpGet("services")]
    public async Task<List<ApiServiceInfoVM>> AllServices()
    {
        var services = await _serviceInfoService.GetAllServiceInfoAsync();
        var vms = new List<ApiServiceInfoVM>();
        foreach (var serviceInfo in services)
        {
            var vm = serviceInfo.ToApiServiceInfoVM();

            vms.Add(vm);
        }

        return vms;
    }

    [HttpGet("services/online")]
    public async Task<List<ApiServiceInfoVM>> OnlineServices()
    {
        var services = await _serviceInfoService.GetOnlineServiceInfoAsync();
        var vms = new List<ApiServiceInfoVM>();
        foreach (var serviceInfo in services)
        {
            var vm = serviceInfo.ToApiServiceInfoVM();

            vms.Add(vm);
        }

        return vms;
    }

    [HttpGet("services/offline")]
    public async Task<List<ApiServiceInfoVM>> OfflineServices()
    {
        var services = await _serviceInfoService.GetOfflineServiceInfoAsync();
        var vms = new List<ApiServiceInfoVM>();
        foreach (var serviceInfo in services)
        {
            var vm = serviceInfo.ToApiServiceInfoVM();

            vms.Add(vm);
        }

        return vms;
    }
}
