using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgileConfig.Server.Apisite.Controllers.api
{
    [TypeFilter(typeof(AppBasicAuthenticationAttribute))]
    [Route("api/[controller]")]
    public class ConfigController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IAppService _appService;
        private readonly IModifyLogService _modifyLogService;
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IServerNodeService _serverNodeService;
        private readonly ISysLogService _sysLogService;
        private readonly IAppBasicAuthService _appBasicAuthService;

        public ConfigController(
            IConfigService configService,
            IAppService appService,
            IModifyLogService modifyLogService,
              IRemoteServerNodeProxy remoteServerNodeProxy,
                                IServerNodeService serverNodeService,
                                ISysLogService sysLogService,
                                IAppBasicAuthService appBasicAuthService)
        {
            _configService = configService;
            _appService = appService;
            _modifyLogService = modifyLogService;
            _remoteServerNodeProxy = remoteServerNodeProxy;
            _serverNodeService = serverNodeService;
            _sysLogService = sysLogService;
            _appBasicAuthService = appBasicAuthService;
        }

        /// <summary>
        /// 根据appid查所有发布的配置项
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet("app/{appId}")]
        public async Task<ActionResult<List<ConfigVM>>> GetAppConfig(string appId)
        {
            var app = await _appService.GetAsync(appId);
            if (!app.Enabled)
            {
                return NotFound();
            }

            var configs = await _configService.GetPublishedConfigsByAppIdWithInheritanced(appId);

            var vms = configs.Select(c =>
            {
                return new ConfigVM()
                {
                    Id = c.Id,
                    AppId = c.AppId,
                    Group = c.Group,
                    Key = c.Key,
                    Value = c.Value,
                    Status = c.Status
                };
            });

            return vms.ToList();
        }

        [HttpGet()]
        public async Task<ActionResult<List<ConfigVM>>> GetConfigs()
        {
            var appId = _appBasicAuthService.GetAppIdSecret(Request).Item1;

            var configs = await _configService.GetByAppId(appId);

            return configs.Select(config => new ConfigVM()
            {
                Id = config.Id,
                AppId = config.AppId,
                Group = config.Group,
                Key = config.Key,
                Value = config.Value,
                Status = config.Status,
                Description = config.Description,
                OnlineStatus = config.OnlineStatus
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConfigVM>> GetConfig(string id)
        {
            var config = await _configService.GetAsync(id);
            if (config == null || config.Status == ConfigStatus.Deleted)
            {
                return NotFound();
            }

            return new ConfigVM()
            {
                Id = config.Id,
                AppId = config.AppId,
                Group = config.Group,
                Key = config.Key,
                Value = config.Value,
                Status = config.Status,
                Description = config.Description,
                OnlineStatus = config.OnlineStatus
            };
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConfigVM model)
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

            var ctrl = new Controllers.ConfigController(
                _configService,
                _modifyLogService,
                _remoteServerNodeProxy,
                _serverNodeService,
                _sysLogService,
                _appService
                );

            var result = (await ctrl.Add(model)) as JsonResult;

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] ConfigVM model)
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

            var ctrl = new Controllers.ConfigController(
                _configService,
                _modifyLogService,
                _remoteServerNodeProxy,
                _serverNodeService,
                _sysLogService,
                _appService
                );

            model.Id = id;
            var result = (await ctrl.Edit(model)) as JsonResult;

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var ctrl = new Controllers.ConfigController(
                _configService,
                _modifyLogService,
                _remoteServerNodeProxy,
                _serverNodeService,
                _sysLogService,
                _appService
                );

            var result = (await ctrl.Delete(id)) as JsonResult;

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

        [HttpPost("publish/{id}")]
        public async Task<IActionResult> Publish(string id)
        {
            var ctrl = new Controllers.ConfigController(
                _configService,
                _modifyLogService,
                _remoteServerNodeProxy,
                _serverNodeService,
                _sysLogService,
                _appService
                );

            var result = (await ctrl.Publish(id)) as JsonResult;

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

        [HttpPost("offline/{id}")]
        public async Task<IActionResult> Offline(string id)
        {
            var ctrl = new Controllers.ConfigController(
                _configService,
                _modifyLogService,
                _remoteServerNodeProxy,
                _serverNodeService,
                _sysLogService,
                _appService
                );

            var result = (await ctrl.Offline(id)) as JsonResult;

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

        private (bool, string) CheckRequired(ConfigVM model)
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
