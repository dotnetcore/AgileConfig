using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RegisterCenterController> _logger;

        public RegisterCenterController(IRegisterCenterService registerCenterService
            ,IServiceInfoService serviceInfoService,
            ILoggerFactory loggerFactory
            )
        {
            _registerCenterService = registerCenterService;
            _serviceInfoService = serviceInfoService;
            _logger = loggerFactory.CreateLogger<RegisterCenterController>();
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
            entity.AlarmUrl = model.AlarmUrl;
            entity.HeartBeatMode = model.HeartBeatMode;
            entity.MetaData = model.MetaData is null ? "[]" : JsonConvert.SerializeObject(model.MetaData);
            entity.RegisterWay = RegisterWay.Auto;
            
            var id = await _registerCenterService.RegisterAsync(entity);

            //send a message to notify other services
            dynamic param = new ExpandoObject();
            param.ServiceId = model.ServiceId;
            param.ServiceName = model.ServiceName;
            param.UniqueId = id;
            TinyEventBus.Instance.Fire(EventKeys.REGISTER_A_SERVICE,param);
            
            return new RegisterResultVM
            {
                UniqueId = id
            };
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<RegisterResultVM>> UnRegister(string id, [FromBody]RegisterServiceInfoVM vm)
        {
            var entity = await _serviceInfoService.GetByUniqueIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            
            var result = await _registerCenterService.UnRegisterAsync(id);
            if (!result)
            {
                if (!string.IsNullOrEmpty(vm?.ServiceId))
                { 
                    result = await _registerCenterService.UnRegisterByServiceIdAsync(vm.ServiceId);
                }
            }

            if (result)
            {
                //send a message to notify other services
                dynamic param = new ExpandoObject();
                param.ServiceId = entity.ServiceId;
                param.ServiceName = entity.ServiceName;
                param.UniqueId = id;
                TinyEventBus.Instance.Fire(EventKeys.UNREGISTER_A_SERVICE,param);
            }
            
            return new RegisterResultVM
            {
                UniqueId = id,
            };
        }

        [HttpPost("heartbeat")]
        public async Task<ActionResult<HeartbeatResultVM>> Heartbeat([FromBody]HeartbeatParam param)
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

            if (serviceHeartbeatResult)
            {
                var md5 = await _serviceInfoService.ServicesMD5Cache();
                return new HeartbeatResultVM()
                {
                    Action = ActionConst.Ping,
                    Data = md5,
                    Module = ActionModule.RegisterCenter
                };
            }

            return NotFound();
        }
        
        [HttpGet("services")]
        public async Task<List<ServiceInfoVM>> AllServices()
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
                    MetaData = new List<string>(),
                    Status = serviceInfo.Status
                };
                try
                {
                    vm.MetaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"deserialize meta data error, serviceId:{serviceInfo.ServiceId}");
                }
                vms.Add(vm);
            }

            return vms;
        }
        
        [HttpGet("services/online")]
        public async Task<List<ServiceInfoVM>> OnlineServices()
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
                    MetaData = new List<string>(),
                    Status = serviceInfo.Status
                };
                try
                {
                    vm.MetaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"deserialize meta data error, serviceId:{serviceInfo.ServiceId}");
                }
                vms.Add(vm);
            }

            return vms;
        }
        
        [HttpGet("services/offline")]
        public async Task<List<ServiceInfoVM>> OfflineServices()
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
                    MetaData = new List<string>(),
                    Status = serviceInfo.Status
                };
                try
                {
                    vm.MetaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"deserialize meta data error, serviceId:{serviceInfo.ServiceId}");
                }
                vms.Add(vm);
            }

            return vms;
        }
    }
}
