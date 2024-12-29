using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models.Mapping;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AgileConfig.Server.Apisite.Controllers.api
{
    /// <summary>
    /// 应用操作接口
    /// </summary>
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [Route("api/[controller]")]
    public class AppController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IAppService _appService;

        private readonly Controllers.AppController _appController;
        private readonly Controllers.ConfigController _configController;

        public AppController(IAppService appService,
            IConfigService configService,
            Controllers.AppController appController,
            Controllers.ConfigController configController)
        {
            _appService = appService;
            _configService = configService;

            _appController = appController;
            _configController = configController;
        }

        /// <summary>
        /// 获取所有应用
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiAppVM>>> GetAll()
        {
            var apps = await _appService.GetAllAppsAsync();
            var vms = apps.Select(x => x.ToApiAppVM());

            return Json(vms);
        }

        /// <summary>
        /// 根据id获取应用
        /// </summary>
        /// <param name="id">应用id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiAppVM>> GetById(string id)
        {
            var actionResult = await _appController.Get(id);
            var status = actionResult as IStatusCodeActionResult;

            var result = actionResult as JsonResult;
            dynamic obj = result?.Value;

            if (obj?.success ?? false)
            {
                AppVM appVM = obj.data;
                return Json(appVM.ToApiAppVM());
            }

            Response.StatusCode = status.StatusCode.Value;
            return Json(new
            {
                obj?.message
            });
        }

        /// <summary>
        /// 添加应用
        /// </summary>
        /// <param name="model">应用模型</param>
        /// <returns></returns>
        [ProducesResponseType(201)]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "App.Add", Functions.App_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ApiAppVM model)
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

            _appController.ControllerContext.HttpContext = HttpContext;

            var result = (await _appController.Add(model.ToAppVM())) as JsonResult;

            dynamic obj = result?.Value;

            if (obj?.success == true)
            {
                return Created("/api/app/" + obj.data.Id, "");
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj?.message
            });
        }

        /// <summary>
        /// 编辑应用
        /// </summary>
        /// <param name="id">应用id</param>
        /// <param name="model">编辑后的应用模型</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "App.Edit", Functions.App_Edit })]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] ApiAppVM model)
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

            _appController.ControllerContext.HttpContext = HttpContext;

            model.Id = id;
            var actionResult = await _appController.Edit(model.ToAppVM());
            var status = actionResult as IStatusCodeActionResult;
            var result = actionResult as JsonResult;

            dynamic obj = result?.Value;
            if (obj?.success ?? false)
            {
                return Ok();
            }

            Response.StatusCode = status.StatusCode.Value;
            return Json(new
            {
                obj?.message
            });
        }

        /// <summary>
        /// 删除应用
        /// </summary>
        /// <param name="id">应用id</param>
        /// <returns></returns>
        [ProducesResponseType(204)]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "App.Delete", Functions.App_Delete })]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _appController.ControllerContext.HttpContext = HttpContext;

            var actionResult = await _appController.Delete(id);
            var status = actionResult as IStatusCodeActionResult;
            var result = actionResult as JsonResult;

            dynamic obj = result?.Value;
            if (obj?.success ?? false)
            {
                return NoContent();
            }

            Response.StatusCode = status.StatusCode.Value;
            return Json(new
            {
                obj?.message
            });
        }

        /// <summary>
        /// 发布某个应用的待发布配置项
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "Config.Publish_API", Functions.Config_Publish })]
        [HttpPost("publish")]
        public async Task<IActionResult> Publish(string appId, EnvString env)
        {
            _configController.ControllerContext.HttpContext = HttpContext;

            var actionResult = await _configController.Publish(new PublishLogVM()
            {
                AppId = appId
            }, env);
            var status = actionResult as IStatusCodeActionResult;
            var result = actionResult as JsonResult;

            dynamic obj = result?.Value;
            if (obj?.success ?? false)
            {
                return Ok();
            }

            Response.StatusCode = status.StatusCode.Value;
            return Json(new
            {
                obj?.message
            });
        }

        /// <summary>
        /// 查询某个应用的发布历史
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [HttpGet("Publish_History")]
        public async Task<ActionResult<IEnumerable<ApiPublishTimelineVM>>> PublishHistory(string appId, EnvString env)
        {
            ArgumentException.ThrowIfNullOrEmpty(appId);

            var history = await _configService.GetPublishTimelineHistoryAsync(appId, env.Value);

            history = history.OrderByDescending(x => x.Version).ToList();

            var vms = history.Select(x => x.ToApiPublishTimelimeVM());

            return Json(vms);
        }

        /// <summary>
        /// 回滚某个应用的发布版本，回滚到 historyId 指定的时刻
        /// </summary>
        /// <param name="historyId">发布历史</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "Config.Rollback_API", Functions.Config_Publish })]
        [HttpPost("rollback")]
        public async Task<IActionResult> Rollback(string historyId, EnvString env)
        {
            _configController.ControllerContext.HttpContext = HttpContext;

            var actionResult = await _configController.Rollback(historyId, env);
            var status = actionResult as IStatusCodeActionResult;
            var result = actionResult as JsonResult;

            dynamic obj = result?.Value;
            if (obj?.success ?? false)
            {
                return Ok();
            }

            Response.StatusCode = status.StatusCode.Value;
            return Json(new
            {
                obj?.message
            });
        }

        private (bool, string) CheckRequired(ApiAppVM model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return (false, "Id不能为空");
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                return (false, "Name不能为空");
            }

            return (true, "");
        }
    }
}
