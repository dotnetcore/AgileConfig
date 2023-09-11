using AgileConfig.Server.Apisite.Controllers.api.Models;
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
    /// <summary>
    /// 节点操作接口
    /// </summary>
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

        /// <summary>
        /// 获取所有节点
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiNodeVM>>> GetAll()
        {
            var nodes = await _serverNodeService.GetAllNodesAsync();

            var vms = nodes.Select(x=> new ApiNodeVM
            { 
                Address = x.Address,
                Remark = x.Remark,
                LastEchoTime = x.LastEchoTime,
                Status= x.Status
            });

            return Json(vms);
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="model">节点模型</param>
        /// <returns></returns>
        [ProducesResponseType(201)]
        [TypeFilter(typeof(PremissionCheckByBasicAttribute), Arguments = new object[] { "Node.Add", Functions.Node_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ApiNodeVM model)
        {
            var requiredResult = CheckRequired(model);

            if (!requiredResult.Item1)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    message = "添加节点失败"
                });
            }

            var ctrl = new ServerNodeController(_serverNodeService, _sysLogService, _remoteServerNodeProxy);
            ctrl.ControllerContext.HttpContext = HttpContext;
            var result = (await ctrl.Add(new ServerNodeVM { 
                Address = model.Address,
                Remark = model.Remark,
                LastEchoTime = model.LastEchoTime,
                Status = model.Status
            })) as JsonResult;

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

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="address">节点地址</param>
        /// <returns></returns>
        [ProducesResponseType(204)]
        [TypeFilter(typeof(PremissionCheckByBasicAttribute), Arguments = new object[] { "Node.Delete", Functions.Node_Delete })]
        [HttpDelete()]
        public async Task<IActionResult> Delete([FromQuery] string address)
        {
            var ctrl = new ServerNodeController(_serverNodeService, _sysLogService, _remoteServerNodeProxy);
            ctrl.ControllerContext.HttpContext = HttpContext;
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

        private (bool, string) CheckRequired(ApiNodeVM model)
        {
            if (string.IsNullOrEmpty(model.Address))
            {
                return (false, "Address不能为空");
            }

            return (true, "");
        }
    }
}
