using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
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
        private readonly IUserService _userService;
        private readonly IMemoryCache _cacheMemory;

        public ConfigController(
            IConfigService configService,
            IAppService appService,
            IUserService userService,
            IMemoryCache cacheMemory)
        {
            _configService = configService;
            _appService = appService;
            _userService = userService;
            _cacheMemory = cacheMemory;
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
        public async Task<ActionResult<List<ApiConfigVM>>> GetAppConfig(string appId, [FromQuery]string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }
            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var app = await _appService.GetAsync(appId);
            if (!app.Enabled)
            {
                return NotFound();
            }

            var cacheKey = $"ConfigController_APPCONFIG_{appId}_{env}";
            _cacheMemory.TryGetValue(cacheKey, out List<ApiConfigVM> configs);
            if (configs != null)
            {
                return configs;
            }
            
            var appConfigs = await _configService.GetPublishedConfigsByAppIdWithInheritanced(appId, env);
            var vms = appConfigs.Select(c =>
            {
                return new ApiConfigVM()
                {
                    Id = c.Id,
                    AppId = c.AppId,
                    Group = c.Group,
                    Key = c.Key,
                    Value = c.Value,
                    Status = c.Status,
                    OnlineStatus = c.OnlineStatus,
                    EditStatus = c.EditStatus
                };
            }).ToList();
            
            //增加5s的缓存，防止同一个app同时启动造成db的压力过大
            var cacheOp = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(5));
            _cacheMemory.Set(cacheKey, vms, cacheOp);
            
            return vms;
        }

        /// <summary>
        /// 根据应用id查找配置，这些配置有可能是未发布的配置 。请跟 config/app/{appId} 接口加以区分。
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [HttpGet()]
        public async Task<ActionResult<List<ApiConfigVM>>> GetConfigs(string appId, string env)
        {
            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.GetByAppIdAsync(appId, env);

            return configs.Select(config => new ApiConfigVM()
            {
                Id = config.Id,
                AppId = config.AppId,
                Group = config.Group,
                Key = config.Key,
                Value = config.Value,
                Status = config.Status,
                Description = config.Description,
                OnlineStatus = config.OnlineStatus,
                EditStatus = config.EditStatus
            }).ToList();
        }

        /// <summary>
        /// 根据编号获取配置项的详情
        /// </summary>
        /// <param name="id">配置id</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiConfigVM>> GetConfig(string id, string env)
        {
            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var config = await _configService.GetAsync(id, env);
            if (config == null || config.Status == ConfigStatus.Deleted)
            {
                return NotFound();
            }

            return new ApiConfigVM()
            {
                Id = config.Id,
                AppId = config.AppId,
                Group = config.Group,
                Key = config.Key,
                Value = config.Value,
                Status = config.Status,
                Description = config.Description,
                OnlineStatus = config.OnlineStatus,
                EditStatus = config.EditStatus
            };
        }

        /// <summary>
        /// 添加一个配置项
        /// </summary>
        /// <param name="model">配置模型</param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        [ProducesResponseType(201)]
        [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
        [TypeFilter(typeof(PremissionCheckByBasicAttribute), Arguments = new object[] { "Config.Add", Functions.Config_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ApiConfigVM model, string env)
        {
            var requiredResult = CheckRequired(model);
            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            if (!requiredResult.Item1)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    message = requiredResult.Item2
                });
            }

            var ctrl = new Controllers.ConfigController(
                _configService,
                _appService,
                _userService
                );
            ctrl.ControllerContext.HttpContext = HttpContext;

            var result = (await ctrl.Add(new ConfigVM()
            {
                Id = model.Id,
                AppId = model.AppId,
                Group = model.Group,
                Key = model.Key,
                Value = model.Value,
                Description = model.Description
            }, env)) as JsonResult;

            dynamic obj = result.Value;

            if (obj.success == true)
            {
                return Created("/api/config/" + obj.data.Id, "");
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
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
        [TypeFilter(typeof(PremissionCheckByBasicAttribute), Arguments = new object[] { "Config.Edit", Functions.Config_Edit })]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] ApiConfigVM model, string env)
        {
            var requiredResult = CheckRequired(model);
            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            if (!requiredResult.Item1)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    message = requiredResult.Item2
                });
            }

            var ctrl = new Controllers.ConfigController(
                _configService,
                _appService,
                _userService
                );
            ctrl.ControllerContext.HttpContext = HttpContext;

            model.Id = id;
            var result = (await ctrl.Edit(new ConfigVM()
            {
                Id = model.Id,
                AppId = model.AppId,
                Group = model.Group,
                Key = model.Key,
                Value = model.Value,
                Description = model.Description
            }, env)) as JsonResult;

            dynamic obj = result.Value;
            if (obj.success == true)
            {
                return Ok();
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
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
        [TypeFilter(typeof(PremissionCheckByBasicAttribute), Arguments = new object[] { "Config.Delete", Functions.Config_Delete })]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, string env)
        {
            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var ctrl = new Controllers.ConfigController(
                _configService,
                _appService,
                _userService
                );
            ctrl.ControllerContext.HttpContext = HttpContext;

            var result = (await ctrl.Delete(id, env)) as JsonResult;

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

        private (bool, string) CheckRequired(ApiConfigVM model)
        {
            if (string.IsNullOrEmpty(model.Key))
            {
                return (false, "Key不能为空");
            }
            if (string.IsNullOrEmpty(model.Value))
            {
                return (false, "Value不能为空");
            }
            if (string.IsNullOrEmpty(model.AppId))
            {
                return (false, "AppId不能为空");
            }

            return (true, "");
        }

    }
}
