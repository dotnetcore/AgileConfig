using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    [ModelVaildate]
    public class ServiceController : Controller
    {
        private readonly IServiceInfoService _serviceInfoService;
        public ServiceController(IServiceInfoService serviceInfoService)
        {
            _serviceInfoService = serviceInfoService;
        }

        public async Task<IActionResult> Search(string serviceName, string serviceId, ServiceAlive? alive, 
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
            if (alive.HasValue)
            {
                query = query.Where(x => x.Alive == alive).ToList();
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
                    Alive = service.Alive,
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    Ip = service.Ip,
                    Port = service.Port,
                    LastHeartBeat = service.LastHeartBeat,
                    MetaData = service.MetaData,
                    RegisterTime = service.RegisterTime,
                    HeartBeatMode = service.HeartBeatMode
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
