using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgileConfig.Server.Apisite.Controllers.api;

[Route("api/[controller]")]
public class ConfigController : Controller
{
    private readonly IAppService _appService;
    private readonly IMemoryCache _cacheMemory;
    private readonly Controllers.ConfigController _configController;
    private readonly IConfigService _configService;
    private readonly IMeterService _meterService;

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
    ///     Retrieve all published configuration items for an application, including inherited entries.
    ///     Note: this endpoint authenticates with appId and secret instead of username and password.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [TypeFilter(typeof(AppBasicAuthenticationAttribute))]
    [HttpGet("app/{appId}")]
    public async Task<ActionResult<List<ApiConfigVM>>> GetAppConfig(string appId, [FromQuery] EnvString env)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        var idInHeader = Encrypt.UnboxBasicAuth(HttpContext.Request).Item1;

        if (appId != idInHeader)
        {
            await Response.WriteAsync("The AppId does not match the ID in Basic Authentication.");
            return BadRequest();
        }

        var app = await _appService.GetAsync(appId);
        if (!app.Enabled) return NotFound();

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

        _meterService.PullAppConfigCounter?.Add(1, new KeyValuePair<string, object>("appId", appId),
            new KeyValuePair<string, object>("env", env));

        return cache.Configs;
    }

    /// <summary>
    ///     Retrieve configuration items for an application, which may include unpublished items.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [HttpGet]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<ActionResult<List<ApiConfigVM>>> GetConfigs(string appId, EnvString env)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        var configs = await _configService.GetByAppIdAsync(appId, env.Value);

        return configs.Select(x => x.ToApiConfigVM()).ToList();
    }

    /// <summary>
    ///     Get configuration details by identifier.
    /// </summary>
    /// <param name="id">Configuration identifier.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [HttpGet("{id}")]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<ActionResult<ApiConfigVM>> GetConfig(string id, EnvString env)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var config = await _configService.GetAsync(id, env.Value);
        if (config == null || config.Status == ConfigStatus.Deleted) return NotFound();

        return config.ToApiConfigVM();
    }

    /// <summary>
    ///     Create a configuration item.
    /// </summary>
    /// <param name="model">Configuration payload.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [ProducesResponseType(201)]
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Config_Add })]
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

        var result = await _configController.Add(model.ToConfigVM(), env) as JsonResult;

        dynamic obj = result?.Value;

        if (obj?.success == true) return Created("/api/config/" + obj.data.Id, "");

        Response.StatusCode = 400;
        return Json(new
        {
            obj?.message
        });
    }

    /// <summary>
    ///     Edit a configuration item.
    /// </summary>
    /// <param name="id">Configuration identifier.</param>
    /// <param name="model">Configuration payload.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Config_Edit })]
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
        var result = await _configController.Edit(model.ToConfigVM(), env) as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success == true) return Ok();

        Response.StatusCode = 400;
        return Json(new
        {
            obj?.message
        });
    }

    /// <summary>
    ///     Delete a configuration item.
    /// </summary>
    /// <param name="id">Configuration identifier.</param>
    /// <param name="env">Target environment.</param>
    /// <returns></returns>
    [ProducesResponseType(204)]
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [TypeFilter(typeof(PermissionCheckByBasicAttribute),
        Arguments = new object[] { Functions.Config_Delete })]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, EnvString env)
    {
        _configController.ControllerContext.HttpContext = HttpContext;

        var result = await _configController.Delete(id, env) as JsonResult;

        dynamic obj = result?.Value;
        if (obj?.success == true) return NoContent();

        Response.StatusCode = 400;
        return Json(new
        {
            obj?.message
        });
    }

    private (bool, string) CheckRequired(ApiConfigVM model)
    {
        if (string.IsNullOrEmpty(model.Key)) return (false, "Key is required");
        if (string.IsNullOrEmpty(model.AppId)) return (false, "AppId is required");

        return (true, "");
    }
}