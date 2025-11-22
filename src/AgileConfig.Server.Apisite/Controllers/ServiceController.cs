using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
[ModelVaildate]
public class ServiceController : Controller
{
    private readonly IRegisterCenterService _registerCenterService;
    private readonly IServiceInfoService _serviceInfoService;
    private readonly ITinyEventBus _tinyEventBus;

    public ServiceController(IServiceInfoService serviceInfoService,
        IRegisterCenterService registerCenterService,
        ITinyEventBus tinyEventBus)
    {
        _serviceInfoService = serviceInfoService;
        _registerCenterService = registerCenterService;
        _tinyEventBus = tinyEventBus;
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Service_Add })]
    public async Task<IActionResult> Add([FromBody] ServiceInfoVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        if (await _serviceInfoService.GetByServiceIdAsync(model.ServiceId) != null)
            return Json(new
            {
                success = false,
                message = Messages.ServiceAlreadyExists
            });

        var service = new ServiceInfo();
        service.Ip = model.Ip;
        service.Port = model.Port;
        service.AlarmUrl = model.AlarmUrl;
        service.CheckUrl = model.CheckUrl;
        service.MetaData = model.MetaData;
        service.ServiceId = model.ServiceId;
        service.ServiceName = model.ServiceName;
        service.RegisterWay = RegisterWay.Manual;
        service.HeartBeatMode = model.HeartBeatMode;
        var uniqueId = await _registerCenterService.RegisterAsync(service);

        _tinyEventBus.Fire(new ServiceRegisteredEvent(uniqueId));

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

        var service = await _serviceInfoService.GetByUniqueIdAsync(id);
        if (service == null)
            return Json(new
            {
                success = false,
                message = Messages.ServiceNotFound
            });

        await _registerCenterService.UnRegisterAsync(id);

        _tinyEventBus.Fire(new ServiceUnRegisterEvent(service.Id));

        return Json(new
        {
            success = true
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