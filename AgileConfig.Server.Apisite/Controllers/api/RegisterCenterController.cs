using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using System;
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

        public RegisterCenterController(IRegisterCenterService registerCenterService)
        {
            _registerCenterService = registerCenterService;
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

        [HttpPost("heartbeat/{id}")]
        public async Task<string> Heartbeat(string id)
        {
            await _registerCenterService.ReceiveHeartbeatAsync(id);

            return "ok";
        }
    }
}
