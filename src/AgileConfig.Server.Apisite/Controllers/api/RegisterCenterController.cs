using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Event;

namespace AgileConfig.Server.Apisite.Controllers.api
{
    /// <summary>
    /// Service registration center API.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterCenterController : Controller
    {
        private readonly IRegisterCenterService _registerCenterService;
        private readonly IServiceInfoService _serviceInfoService;
        private readonly ITinyEventBus _tinyEventBus;

        public RegisterCenterController(IRegisterCenterService registerCenterService
            , IServiceInfoService serviceInfoService,
            ITinyEventBus tinyEventBus
            )
        {
            _registerCenterService = registerCenterService;
            _serviceInfoService = serviceInfoService;
            _tinyEventBus = tinyEventBus;
        }

        [HttpPost]
        public async Task<RegisterResultVM> Register([FromBody] RegisterServiceInfoVM model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var entity = model.ToServiceInfo();
            entity.RegisterWay = RegisterWay.Auto;

            var id = await _registerCenterService.RegisterAsync(entity);

            _tinyEventBus.Fire(new ServiceRegisteredEvent(id));

            return new RegisterResultVM
            {
                UniqueId = id
            };
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<RegisterResultVM>> UnRegister(string id, [FromBody] RegisterServiceInfoVM vm)
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
                _tinyEventBus.Fire(new ServiceUnRegisterEvent(id));
            }

            return new RegisterResultVM
            {
                UniqueId = id,
            };
        }

        [HttpPost("heartbeat")]
        public async Task<ActionResult<HeartbeatResultVM>> Heartbeat([FromBody] HeartbeatParam param)
        {
            ArgumentNullException.ThrowIfNull(param);

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
        public async Task<List<ApiServiceInfoVM>> AllServices()
        {
            var services = await _serviceInfoService.GetAllServiceInfoAsync();
            var vms = new List<ApiServiceInfoVM>();
            foreach (var serviceInfo in services)
            {
                var vm =  serviceInfo.ToApiServiceInfoVM();

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
}
