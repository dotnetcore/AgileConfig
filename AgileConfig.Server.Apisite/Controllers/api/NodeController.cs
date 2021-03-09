using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers.api
{
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [Route("api/[controller]")]
    public class NodeController : Controller
    {
        private readonly IServerNodeService _serverNodeService;
        private readonly ISysLogService _sysLogService;
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        public NodeController(IServerNodeService serverNodeService, ISysLogService sysLogService, IRemoteServerNodeProxy remoteServerNodeProxy)
        {
            _serverNodeService = serverNodeService;
            _sysLogService = sysLogService;
            _remoteServerNodeProxy = remoteServerNodeProxy;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var nodes = await _serverNodeService.GetAllNodesAsync();

            var vms = nodes.Select(x=> new ServerNodeVM { 
                Address = x.Address,
                Remark = x.Remark,
                LastEchoTime = x.LastEchoTime,
                Status= x.Status
            });

            return Json(vms);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ServerNodeVM model)
        {
            var requiredResult = CheckRequired(model);

            if (!requiredResult.Item1)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    message = requiredResult.Item2
                });
            }

            var ctrl = new ServerNodeController(_serverNodeService, _sysLogService, _remoteServerNodeProxy);
            var result = (await ctrl.Add(model)) as JsonResult;

            dynamic obj = result.Value;
            if (obj.success == true)
            {
                return Created("","");
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
            });
        }

        [HttpDelete()]
        public async Task<IActionResult> Delete([FromQuery] string address)
        {
            var ctrl = new ServerNodeController(_serverNodeService, _sysLogService, _remoteServerNodeProxy);
            var result = (await ctrl.Delete(new ServerNodeVM { Address = address })) as JsonResult;

            dynamic obj = result.Value;
            if (obj.success == true)
            {
                return NoContent();
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
            });
        }

        private (bool, string) CheckRequired(ServerNodeVM model)
        {
            if (string.IsNullOrEmpty(model.Address))
            {
                return (false, "Address不能为空");
            }

            return (true, "");
        }
    }
}
