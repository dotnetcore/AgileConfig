using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
[ModelVaildate]
public class ServiceController : Controller
{
    private readonly IServiceInstanceManagementService _serviceInstanceManagementService;
    private readonly IServiceInfoService _serviceInfoService;

    public ServiceController(
        IServiceInfoService serviceInfoService,
        IServiceInstanceManagementService serviceInstanceManagementService)
    {
        _serviceInfoService = serviceInfoService;
        _serviceInstanceManagementService = serviceInstanceManagementService;
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Service_Add })]
    public async Task<IActionResult> Add([FromBody] ServiceInfoVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var result = await _serviceInstanceManagementService.RegisterAsync(new RegisterServiceInstanceCommand(
            model.ServiceId,
            model.ServiceName,
            model.Ip,
            model.Port,
            model.MetaData,
            model.CheckUrl,
            model.AlarmUrl,
            model.HeartBeatMode,
            RegisterWay.Manual,
            RejectExisting: true,
            AcceptMissingIdentifier: true));

        if (!result.Succeeded)
            return Json(new
            {
                success = false,
                message = result.Error == ApplicationError.Conflict ? Messages.ServiceAlreadyExists : ""
            });

        return Json(new
        {
            success = true
        });
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Service_Delete })]
    public async Task<IActionResult> Remove(string id)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

        var result = await _serviceInstanceManagementService.UnregisterAsync(
            id,
            succeedWhenRemovalFails: true);
        if (!result.Succeeded && result.Error == ApplicationError.NotFound)
            return Json(new
            {
                success = false,
                message = Messages.ServiceNotFound
            });

        return Json(new
        {
            success = result.Succeeded || result.Error == ApplicationError.OperationFailed
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Service_Read })]
    public async Task<IActionResult> Search(string serviceName, string serviceId, ServiceStatus? status,
        string sortField, string ascOrDesc,
        int current = 1, int pageSize = 20)
    {
        if (current < 1) throw new ArgumentException(Messages.CurrentCannotBeLessThanOneService);
        if (pageSize < 1) throw new ArgumentException(Messages.PageSizeCannotBeLessThanOneService);

        var query = await _serviceInfoService.GetAllServiceInfoAsync();
        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(x => x.ServiceName.Contains(serviceName)).ToList();
        if (!string.IsNullOrWhiteSpace(serviceId)) query = query.Where(x => x.ServiceId.Contains(serviceId)).ToList();
        if (status.HasValue) query = query.Where(x => x.Status == status).ToList();

        query = query.OrderByDescending(x => x.RegisterTime).ToList();

        if (sortField == "registerTime")
        {
            if (ascOrDesc.StartsWith("asc"))
                query = query.OrderBy(x => x.RegisterTime).ToList();
            else
                query = query.OrderByDescending(x => x.RegisterTime).ToList();
        }

        if (sortField == "serviceName")
        {
            if (ascOrDesc.StartsWith("asc"))
                query = query.OrderBy(x => x.ServiceName).ToList();
            else
                query = query.OrderByDescending(x => x.ServiceName).ToList();
        }

        var count = query.Count;
        var page = query.Skip((current - 1) * pageSize).Take(pageSize).ToList();

        var serviceVMs = new List<ServiceInfoVM>();
        foreach (var service in page)
            serviceVMs.Add(new ServiceInfoVM
            {
                Id = service.Id,
                Status = service.Status,
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Ip = service.Ip,
                Port = service.Port,
                LastHeartBeat = service.LastHeartBeat,
                MetaData = service.MetaData,
                RegisterTime = service.RegisterTime,
                HeartBeatMode = service.HeartBeatMode,
                CheckUrl = service.CheckUrl,
                AlarmUrl = service.AlarmUrl
            });

        return Json(new
        {
            current,
            pageSize,
            success = true,
            total = count,
            data = serviceVMs
        });
    }
}
