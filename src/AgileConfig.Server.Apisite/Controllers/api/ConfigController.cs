using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgileConfig.Server.Apisite.Controllers.api
{
    [Route("api/[controller]")]
    public class ConfigController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IAppService _appService;
        private readonly IMemoryCache _cacheMemory;
        private readonly IMeterService _meterService;
        private readonly Controllers.ConfigController _configController;

        public ConfigController(
            IConfigService configService,
            IAppService appService,
            IMemoryCache cacheMemory,
            IMeterService meterService,
            Controllers.ConfigController configController
            )
        {
            _configService = configService;
            _appService = appService;
            _cacheMemory = cacheMemory;
            _meterService = meterService;
            _configController = configController;
        }

        /// <summary>
        /// 根据appid查所有发布的配置项 , 包括继承过来的配置项.
        /// 注意： 这个接口用的不是用户名密码的认证，用的是appid + secret的认证
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AppBasicAuthenticationAttribute))]
        [HttpGet("app/{appId}")]
        public async Task<ActionResult<List<ApiConfigVM>>> GetAppConfig(string appId, [FromQuery] EnvString env)
        {
            ArgumentException.ThrowIfNullOrEmpty(appId);

            var app = await _appService.GetAsync(appId);
            if (!app.Enabled)
            {
                return NotFound();
            }

            var cacheKey = $"ConfigController_AppConfig_{appId}_{env.Value}";
            AppConfigsCache cache = null;
            _cacheMemory?.TryGetValue(cacheKey, out cache);

            if (cache == null)
            {
                cache = new AppConfigsCache();

                var publishTimelineId = await _configService.GetLastPublishTimelineVirtualIdAsync(appId, env.Value);
                var appConfigs = await _configService.GetPublishedConfigsByAppIdWithInheritance(appId, env.Value);
                var vms = appConfigs.Select(x => x.ToApiConfigVM()).ToList();

                cache.Key = cacheKey;
                cache.Configs = vms;
                cache.VirtualId = publishTimelineId;

                //cache 5 seconds to avoid too many db query
                var cacheOp = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(5));
                _cacheMemory?.Set(cacheKey, cache, cacheOp);
            }
            
            Response?.Headers?.Append("publish-time-line-id", cache.VirtualId);

            _meterService.PullAppConfigCounter?.Add(1, new("appId", appId), new("env", env));

            return cache.Configs;
        }

        /// <summary>
        /// 根据应用id查找配置，这些配置有可能是未发布的配置 。请跟 config/app/{appId} 接口加以区分。
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [HttpGet()]
        public async Task<ActionResult<List<ApiConfigVM>>> GetConfigs(string appId, EnvString env)
        {
            ArgumentException.ThrowIfNullOrEmpty(appId);

            var configs = await _configService.GetByAppIdAsync(appId, env.Value);

            return configs.Select(x => x.ToApiConfigVM()).ToList();
        }

        /// <summary>
        /// 根据编号获取配置项的详情
        /// </summary>
        /// <param name="id">配置id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiConfigVM>> GetConfig(string id, EnvString env)
        {
            ArgumentException.ThrowIfNullOrEmpty(id);

            var config = await _configService.GetAsync(id, env.Value);
            if (config == null || config.Status == ConfigStatus.Deleted)
            {
                return NotFound();
            }

            return config.ToApiConfigVM();
        }

        /// <summary>
        /// 添加一个配置项
        /// </summary>
        /// <param name="model">配置模型</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [ProducesResponseType(201)]
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "Config.Add", Functions.Config_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ApiConfigVM model, EnvString env)
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

            _configController.ControllerContext.HttpContext = HttpContext;

            var result = (await _configController.Add(model.ToConfigVM(), env)) as JsonResult;

            dynamic obj = result?.Value;

            if (obj?.success == true)
            {
                return Created("/api/config/" + obj.data.Id, "");
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj?.message
            });
        }

        /// <summary>
        /// 编辑一个配置
        /// </summary>
        /// <param name="id">编号</param>
        /// <param name="model">模型</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "Config.Edit", Functions.Config_Edit })]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] ApiConfigVM model, EnvString env)
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

            _configController.ControllerContext.HttpContext = HttpContext;
            model.Id = id;
            var result = (await _configController.Edit(model.ToConfigVM(), env)) as JsonResult;

            dynamic obj = result?.Value;
            if (obj?.success == true)
            {
                return Ok();
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj?.message
            });
        }

        /// <summary>
        /// 删除一个配置
        /// </summary>
        /// <param name="id">配置id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [ProducesResponseType(204)]
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [TypeFilter(typeof(PermissionCheckByBasicAttribute), Arguments = new object[] { "Config.Delete", Functions.Config_Delete })]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, EnvString env)
        {
            _configController.ControllerContext.HttpContext = HttpContext;

            var result = (await _configController.Delete(id, env)) as JsonResult;

            dynamic obj = result?.Value;
            if (obj?.success == true)
            {
                return NoContent();
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj?.message
            });
        }

        private (bool, string) CheckRequired(ApiConfigVM model)
        {
            if (string.IsNullOrEmpty(model.Key))
            {
                return (false, "Key is required");
            }
            if (string.IsNullOrEmpty(model.AppId))
            {
                return (false, "AppId is required");
            }

            return (true, "");
        }

    }
}
