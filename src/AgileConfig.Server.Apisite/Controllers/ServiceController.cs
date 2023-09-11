using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    [ModelVaildate]
    public class ServiceController : Controller
    {
        private readonly IServiceInfoService _serviceInfoService;
        private readonly IRegisterCenterService _registerCenterService;
        public ServiceController(IServiceInfoService serviceInfoService,IRegisterCenterService registerCenterService)
        {
            _serviceInfoService = serviceInfoService;
            _registerCenterService = registerCenterService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ServiceInfoVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if ((await _serviceInfoService.GetByServiceIdAsync(model.ServiceId)) != null)
            {
                return Json(new
                {
                    success = false,
                    message = "该服务已存在"
                });
            }
            
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

            //send a message to notify other services
            dynamic param = new ExpandoObject();
            param.ServiceId = model.ServiceId;
            param.ServiceName = model.ServiceName;
            param.UniqueId = uniqueId;
            TinyEventBus.Instance.Fire(EventKeys.REGISTER_A_SERVICE,param);
            
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var service = await _serviceInfoService.GetByUniqueIdAsync(id);
            if (service == null)
            {
                return Json(new
                {
                    success = false,
                    message = "该服务不存在"
                });
            }

            await _registerCenterService.UnRegisterAsync(id);

            //send a message to notify other services
            dynamic param = new ExpandoObject();
            param.ServiceId = service.ServiceId;
            param.ServiceName = service.ServiceName;
            param.UniqueId = service.Id;
            TinyEventBus.Instance.Fire(EventKeys.UNREGISTER_A_SERVICE,param);

            return Json(new
            {
                success = true
            });
        }
 
        public async Task<IActionResult> Search(string serviceName, string serviceId, ServiceStatus? status, 
            string sortField, string ascOrDesc,
            int current = 1, int pageSize = 20)
        {
            if (current < 1)
            {
                throw new ArgumentException("current cant less then 1 .");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("pageSize cant less then 1 .");
            }

            var query = await _serviceInfoService.GetAllServiceInfoAsync();
            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                query = query.Where(x => x.ServiceName.Contains(serviceName)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(serviceId))
            {
                query = query.Where(x => x.ServiceId.Contains(serviceId)).ToList();
            }
            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status).ToList();
            }

            query = query.OrderByDescending(x => x.RegisterTime).ToList();
            
            if (sortField == "registerTime")
            {
                if (ascOrDesc.StartsWith("asc"))
                {
                    query = query.OrderBy(x => x.RegisterTime).ToList();
                }
                else
                {
                    query = query.OrderByDescending(x => x.RegisterTime).ToList();
                }
            }
            if (sortField == "serviceName")
            {
                if (ascOrDesc.StartsWith("asc"))
                {
                    query = query.OrderBy(x => x.ServiceName).ToList();
                }
                else
                {
                    query = query.OrderByDescending(x => x.ServiceName).ToList();
                }
            }
            var count = query.Count;
            var page = query.Skip((current - 1) * pageSize).Take(pageSize).ToList();

            var serviceVMs = new List<ServiceInfoVM>();
            foreach (var service in page)
            {
                serviceVMs.Add(new ServiceInfoVM()
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
            }

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
}
