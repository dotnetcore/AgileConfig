﻿using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Controllers.api
{
    /// <summary>
    /// 注册中心接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterCenterController : Controller
    {
        private readonly IRegisterCenterService _registerCenterService;
        private readonly IServiceInfoService _serviceInfoService;

        public RegisterCenterController(IRegisterCenterService registerCenterService
            ,IServiceInfoService serviceInfoService)
        {
            _registerCenterService = registerCenterService;
            _serviceInfoService = serviceInfoService;
        }
       
        [HttpPost]
        public async Task<RegisterResultVM> Register([FromBody]RegisterServiceInfoVM model)
        {
            var entity = new ServiceInfo();
            entity.ServiceId = model.ServiceId;
            entity.ServiceName = model.ServiceName;
            entity.Ip = model.Ip;
            entity.Port = model.Port;
            entity.CheckUrl = model.CheckUrl;
            entity.HeartBeatMode = model.HeartBeatMode;
            entity.MetaData = JsonConvert.SerializeObject(model.MetaData);
            
            var id = await _registerCenterService.RegisterAsync(entity);

            return new RegisterResultVM
            {
                UniqueId = id
            };
        }


        [HttpDelete("{id}")]
        public async Task<RegisterResultVM> UnRegister(string id)
        {
            await _registerCenterService.UnRegisterAsync(id);

            return new RegisterResultVM
            {
                UniqueId = id,
            };
        }

        [HttpPost("heartbeat")]
        public async Task<HeartbeatResultVM> Heartbeat([FromBody]HeartbeatParam param)
        {
            if (param == null)
            {
                throw  new ArgumentNullException(nameof(param));
            }

            bool serviceHeartbeatResult = false;
            if (!string.IsNullOrEmpty(param.UniqueId))
            {
                serviceHeartbeatResult = await _registerCenterService.ReceiveHeartbeatAsync(param.UniqueId);
            }

            return new HeartbeatResultVM
            {
                Success = serviceHeartbeatResult,
                DataVersion = "x"
            };
        }
        
        [HttpGet("services")]
        public async Task<QueryServiceInfoResultVM> AllServices()
        {
            var services = await _serviceInfoService.GetAllServiceInfoAsync();
            var vms = new List<ServiceInfoVM>();
            foreach (var serviceInfo in services)
            {
                var vm = new ServiceInfoVM
                {
                    ServiceId = serviceInfo.ServiceId,
                    ServiceName = serviceInfo.ServiceName,
                    Ip = serviceInfo.Ip,
                    Port = serviceInfo.Port,
                    MetaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData),
                    Status = serviceInfo.Alive
                };
                vms.Add(vm);
            }

            return new QueryServiceInfoResultVM()
            {
                Data = vms,
                DataVersion = "x"
            };
        }
        
        [HttpGet("services/online")]
        public async Task<QueryServiceInfoResultVM> OnlineServices()
        {
            var services = await _serviceInfoService.GetOnlineServiceInfoAsync();
            var vms = new List<ServiceInfoVM>();
            foreach (var serviceInfo in services)
            {
                var vm = new ServiceInfoVM
                {
                    ServiceId = serviceInfo.ServiceId,
                    ServiceName = serviceInfo.ServiceName,
                    Ip = serviceInfo.Ip,
                    Port = serviceInfo.Port,
                    MetaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData),
                    Status = serviceInfo.Alive
                };
                vms.Add(vm);
            }

            return new QueryServiceInfoResultVM()
            {
                Data = vms,
                DataVersion = "x"
            };
        }
        
        [HttpGet("services/offline")]
        public async Task<QueryServiceInfoResultVM> OfflineServices()
        {
            var services = await _serviceInfoService.GetOfflineServiceInfoAsync();
            var vms = new List<ServiceInfoVM>();
            foreach (var serviceInfo in services)
            {
                var vm = new ServiceInfoVM
                {
                    ServiceId = serviceInfo.ServiceId,
                    ServiceName = serviceInfo.ServiceName,
                    Ip = serviceInfo.Ip,
                    Port = serviceInfo.Port,
                    MetaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData),
                    Status = serviceInfo.Alive
                };
                vms.Add(vm);
            }

            return new QueryServiceInfoResultVM()
            {
                Data = vms,
                DataVersion = "x"
            };
        }
    }
}
