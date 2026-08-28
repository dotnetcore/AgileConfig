using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Application.Releases;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
[ModelVaildate]
public class ConfigController : Controller
{
    private readonly IAppService _appService;
    private readonly IConfigService _configService;
    private readonly IConfigurationManagementService _configurationManagementService;
    private readonly IReleaseManagementService _releaseManagementService;
    private readonly ITinyEventBus _tinyEventBus;

    public ConfigController(
        IConfigService configService,
        IAppService appService,
        ITinyEventBus tinyEventBus,
        IConfigurationManagementService configurationManagementService,
        IReleaseManagementService releaseManagementService
    )
    {
        _configService = configService;
        _appService = appService;
        _tinyEventBus = tinyEventBus;
        _configurationManagementService = configurationManagementService;
        _releaseManagementService = releaseManagementService;
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Config_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ConfigVM model, EnvString env)
    {
        ArgumentNullException.ThrowIfNull(model);

        var result = await _configurationManagementService.CreateAsync(new CreateConfigurationCommand(
            model.Id,
            model.AppId,
            model.Group,
            model.Key,
            model.Value,
            model.Description,
            env.Value));

        if (!result.Succeeded)
        {
            var message = result.Error switch
            {
                ApplicationError.NotFound => Messages.AppNotExists(model.AppId),
                ApplicationError.Conflict => Messages.ConfigExists,
                _ => Messages.CreateConfigFailed
            };

            return result.Value == null
                ? Json(new { success = false, message })
                : Json(new { success = false, message, data = result.Value });
        }

        return Json(new
        {
            success = true,
            message = "",
            data = result.Value
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Add })]
    [HttpPost]
    public async Task<IActionResult> AddRange([FromBody] List<ConfigVM> model, EnvString env)
    {
        if (model == null || model.Count == 0) throw new ArgumentNullException("model");

        var configs = await _configService.GetByAppIdAsync(model.First().AppId, env.Value);

        var oldDict = new Dictionary<string, string>();
        configs.ForEach(item => { oldDict.Add(_configService.GenerateKey(item), item.Value); });

        var addConfigs = new List<Config>();
        //judge if json key already in configs
        foreach (var item in model)
        {
            var newkey = item.Key;
            if (!string.IsNullOrEmpty(item.Group)) newkey = $"{item.Group}:{item.Key}";

            if (oldDict.ContainsKey(newkey))
                return Json(new
                {
                    success = false,
                    message = Messages.DuplicateConfig(item.Key)
                });

            var config = new Config();
            config.Id = Guid.NewGuid().ToString("N");
            config.Key = item.Key;
            config.AppId = item.AppId;
            config.Description = item.Description;
            config.Value = item.Value;
            config.Group = item.Group;
            config.Status = ConfigStatus.Enabled;
            config.CreateTime = DateTime.Now;
            config.UpdateTime = null;
            config.OnlineStatus = OnlineStatus.WaitPublish;
            config.EditStatus = EditStatus.Add;
            config.Env = env.Value;

            addConfigs.Add(config);
        }

        var result = await _configService.AddRangeAsync(addConfigs, env.Value);

        if (result)
        {
            var userName = this.GetCurrentUserName();
            addConfigs.ForEach(c => { _tinyEventBus.Fire(new AddConfigSuccessful(c, this.GetCurrentUserName())); });
        }

        return Json(new
        {
            success = result,
            message = !result ? Messages.BatchCreateConfigFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Edit })]
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] ConfigVM model, [FromQuery] EnvString env)
    {
        if (model == null) throw new ArgumentNullException("model");

        var result = await _configurationManagementService.UpdateAsync(new UpdateConfigurationCommand(
            model.Id,
            model.AppId,
            model.Group,
            model.Key,
            model.Value,
            model.Description,
            env.Value));

        if (!result.Succeeded)
            return Json(new
            {
                success = false,
                message = result.Error switch
                {
                    ApplicationError.NotFound =>
                        (await _configService.GetAsync(model.Id, env.Value)) == null
                            ? Messages.ConfigNotFound
                            : Messages.AppNotExists(model.AppId),
                    ApplicationError.Conflict => Messages.ConfigKeyExists,
                    _ => "修改配置失败，请查看错误日志。"
                }
            });

        return Json(new
        {
            success = true,
            message = ""
        });
    }

    [HttpGet]
    public async Task<IActionResult> All(string env)
    {
        ISettingService.IfEnvEmptySetDefault(ref env);

        var configs = await _configService.GetAllConfigsAsync(env);

        return Json(new
        {
            success = true,
            data = configs
        });
    }

    /// <summary>
    ///     Search configurations with multiple filter conditions.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <param name="group">Configuration group.</param>
    /// <param name="key">Configuration key.</param>
    /// <param name="onlineStatus">Filter by online status.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="current">Current page number.</param>
    /// <returns></returns>
    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<IActionResult> Search(string appId, string group, string key, OnlineStatus? onlineStatus,
        string sortField, string ascOrDesc, EnvString env, int pageSize = 20, int current = 1)
    {
        if (pageSize <= 0) throw new ArgumentException("pageSize can not less then 1 .");

        if (current <= 0) throw new ArgumentException("pageIndex can not less then 1 .");

        var configs = await _configService.Search(appId, group, key, env.Value);
        configs = configs.Where(c => c.Status == ConfigStatus.Enabled).ToList();
        if (onlineStatus.HasValue) configs = configs.Where(c => c.OnlineStatus == onlineStatus).ToList();

        if (sortField == "createTime")
        {
            if (ascOrDesc.StartsWith("asc"))
                configs = configs.OrderBy(x => x.CreateTime).ToList();
            else
                configs = configs.OrderByDescending(x => x.CreateTime).ToList();
        }

        if (sortField == "group")
        {
            if (ascOrDesc.StartsWith("asc"))
                configs = configs.OrderBy(x => x.Group).ToList();
            else
                configs = configs.OrderByDescending(x => x.Group).ToList();
        }

        var page = configs.Skip((current - 1) * pageSize).Take(pageSize).ToList();
        var total = configs.Count();

        return Json(new
        {
            current,
            pageSize,
            success = true,
            total,
            data = page
        });
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<IActionResult> Get(string id, EnvString env)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

        var config = await _configService.GetAsync(id, env.Value);

        return Json(new
        {
            success = config != null,
            data = config,
            message = config == null ? "未找到对应的配置项。" : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Delete })]
    [HttpPost]
    public async Task<IActionResult> Delete(string id, EnvString env)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

        var result = await _configurationManagementService.DeleteAsync(
            new DeleteConfigurationCommand(id, env.Value));

        if (!result.Succeeded)
            return Json(new
            {
                success = false,
                message = result.Error == ApplicationError.NotFound
                    ? "未找到对应的配置项。"
                    : "删除配置失败，请查看错误日志"
            });

        return Json(new
        {
            success = true,
            message = ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Delete })]
    [HttpPost]
    public async Task<IActionResult> DeleteSome([FromBody] List<string> ids, EnvString env)
    {
        if (ids == null) throw new ArgumentNullException("ids");

        var deleteConfigs = new List<Config>();

        foreach (var id in ids)
        {
            var config = await _configService.GetAsync(id, env.Value);
            if (config == null)
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });

            config.EditStatus = EditStatus.Deleted;
            config.OnlineStatus = OnlineStatus.WaitPublish;

            var isPublished = await _configService.IsPublishedAsync(config.Id, env.Value);
            if (!isPublished)
                // If it has never been published, remove it directly.
                config.Status = ConfigStatus.Deleted;

            deleteConfigs.Add(config);
        }

        var result = await _configService.UpdateAsync(deleteConfigs, env.Value);
        if (result)
            _tinyEventBus.Fire(new DeleteSomeConfigSuccessful(deleteConfigs.First(), this.GetCurrentUserName()));

        return Json(new
        {
            success = result,
            message = !result ? "删除配置失败，请查看错误日志" : ""
        });
    }


    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Offline })]
    [HttpPost]
    public async Task<IActionResult> Rollback(string publishTimelineId, EnvString env)
    {
        if (string.IsNullOrEmpty(publishTimelineId)) throw new ArgumentNullException("publishTimelineId");

        var result = await _releaseManagementService.RollbackAsync(
            new RollbackConfigurationCommand(publishTimelineId, env.Value));

        return Json(new
        {
            success = result.Succeeded,
            message = !result.Succeeded ? "回滚失败，请查看错误日志。" : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    [HttpGet]
    public async Task<IActionResult> ConfigPublishedHistory(string configId, EnvString env)
    {
        if (string.IsNullOrEmpty(configId)) throw new ArgumentNullException("configId");

        var configPublishedHistory = await _configService.GetConfigPublishedHistory(configId, env.Value);
        var result = new List<object>();

        foreach (var publishDetail in configPublishedHistory.OrderByDescending(x => x.Version))
        {
            var timelineNode =
                await _configService.GetPublishTimeLineNodeAsync(publishDetail.PublishTimelineId, env.Value);
            result.Add(new
            {
                timelineNode,
                config = publishDetail
            });
        }

        return Json(new
        {
            success = true,
            data = result
        });
        ;
    }

    /// <summary>
    ///     Publish all pending configuration items.
    /// </summary>
    /// <returns></returns>
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Publish })]
    [HttpPost]
    public async Task<IActionResult> Publish([FromBody] PublishLogVM model, EnvString env)
    {
        if (model == null) throw new ArgumentNullException("model");

        if (string.IsNullOrEmpty(model.AppId)) throw new ArgumentNullException("appId");

        var result = await _releaseManagementService.PublishAsync(new PublishConfigurationsCommand(
            model.AppId,
            model.Ids,
            model.Log,
            env.Value));

        return Json(new
        {
            success = result.Succeeded,
            message = !result.Succeeded ? "上线配置失败，请查看错误日志" : ""
        });
    }

    /// <summary>
    ///     Preview an uploaded json/jsonc configuration file.
    /// </summary>
    /// <returns></returns>
    public IActionResult PreViewJsonFile()
    {
        var files = Request.Form.Files.ToList();
        if (!files.Any())
            return Json(new
            {
                success = false,
                message = "请上传Json文件"
            });

        var jsonFile = files.First();
        using (var stream = jsonFile.OpenReadStream())
        {
            var parseResult = JsonConfigurationFileParser.ParseWithComments(stream);
            var dict = parseResult.Data;

            var addConfigs = new List<Config>();
            foreach (var key in dict.Keys)
            {
                var newKey = key;
                var group = "";
                var paths = key.Split(":");
                if (paths.Length > 1)
                {
                    // For hierarchical keys, use the last segment as the key and the rest as the group.
                    newKey = paths[paths.Length - 1];
                    group = string.Join(":", paths.ToList().Take(paths.Length - 1));
                }

                var config = new Config();
                config.Key = newKey;
                config.Description = ReadJsoncComment(parseResult.Comments, key);
                config.Value = dict[key];
                config.Group = group;
                config.Id = Guid.NewGuid().ToString();
                addConfigs.Add(config);
            }

            return Json(new
            {
                success = true,
                data = addConfigs
            });
        }
    }

    private static string ReadJsoncComment(IDictionary<string, string> comments, string key)
    {
        if (!comments.TryGetValue(key, out var comment) || string.IsNullOrWhiteSpace(comment)) return "";

        return comment.Length > ConfigService.ConfigDescriptionMaxLength
            ? comment.Substring(0, ConfigService.ConfigDescriptionMaxLength)
            : comment;
    }

    /// <summary>
    ///     Export an application's configurations as a jsonc file.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <returns></returns>
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<IActionResult> ExportJson(string appId, EnvString env)
    {
        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");

        var configs = await _configService.GetByAppIdAsync(appId, env.Value);

        var dict = new Dictionary<string, string>();
        var comments = new Dictionary<string, string>();
        configs.ForEach(x =>
        {
            var key = _configService.GenerateKey(x);
            dict.Add(key, x.Value);
            if (!string.IsNullOrWhiteSpace(x.Description)) comments.Add(key, x.Description);
        });

        var json = DictionaryConvertToJson.ToJsonc(dict, comments);

        return File(Encoding.UTF8.GetBytes(json), "application/json", $"{appId}.jsonc");
    }

    /// <summary>
    ///     Get counts of configuration changes that are waiting to be published.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <returns></returns>
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<IActionResult> WaitPublishStatus(string appId, EnvString env)
    {
        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");

        var configs = await _configService.Search(appId, "", "", env.Value);
        configs = configs.Where(x => x.Status == ConfigStatus.Enabled && x.EditStatus != EditStatus.Commit)
            .ToList();

        var addCount = configs.Count(x => x.EditStatus == EditStatus.Add);
        var editCount = configs.Count(x => x.EditStatus == EditStatus.Edit);
        var deleteCount = configs.Count(x => x.EditStatus == EditStatus.Deleted);

        return Json(new
        {
            success = true,
            data = new
            {
                addCount,
                editCount,
                deleteCount
            }
        });
    }

    /// <summary>
    ///     Retrieve the publish history details for an application.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <returns></returns>
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<IActionResult> PublishHistory(string appId, EnvString env)
    {
        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");

        var history = await _configService.GetPublishDetailListAsync(appId, env.Value);

        var result = new List<object>();
        foreach (var publishDetails in history.GroupBy(x => x.Version).OrderByDescending(g => g.Key))
        {
            var data = publishDetails.ToList();
            result.Add(new
            {
                key = publishDetails.Key,
                timelineNode =
                    await _configService.GetPublishTimeLineNodeAsync(data.FirstOrDefault()?.PublishTimelineId,
                        env.Value),
                list = data
            });
        }

        return Json(new
        {
            success = true,
            data = result
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Edit })]
    public async Task<IActionResult> CancelEdit(string configId, EnvString env)
    {
        if (string.IsNullOrEmpty(configId)) throw new ArgumentNullException("configId");

        var result = await _configService.CancelEdit(new List<string> { configId }, env.Value);

        if (result)
        {
            var config = await _configService.GetAsync(configId, env.Value);
            _tinyEventBus.Fire(new CancelEditConfigSuccessful(config, this.GetCurrentUserName()));
        }

        return Json(new
        {
            success = true
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Edit })]
    public async Task<IActionResult> CancelSomeEdit([FromBody] List<string> ids, EnvString env)
    {
        if (ids == null) throw new ArgumentNullException("ids");

        var result = await _configService.CancelEdit(ids, env.Value);

        if (result)
        {
            var config = await _configService.GetAsync(ids.First(), env.Value);
            _tinyEventBus.Fire(new CancelEditConfigSomeSuccessful(config, this.GetCurrentUserName()));
        }

        return Json(new
        {
            success = true
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Add })]
    [HttpPost]
    public async Task<IActionResult> SyncEnv([FromBody] List<string> toEnvs, [FromQuery] string appId,
        [FromQuery] string currentEnv)
    {
        if (toEnvs == null) throw new ArgumentNullException("toEnvs");

        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");

        if (string.IsNullOrEmpty(currentEnv)) throw new ArgumentNullException("currentEnv");

        var app = await _appService.GetAsync(appId);
        if (app == null)
            return Json(new
            {
                success = false,
                message = $"应用（{appId}）不存在。"
            });

        var result = await _configService.EnvSync(appId, currentEnv, toEnvs);

        return Json(new
        {
            success = result
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<IActionResult> GetKvList(string appId, EnvString env)
    {
        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");

        var configs = await _configService.GetByAppIdAsync(appId, env.Value);
        // When displaying text format, exclude deleted configurations.
        configs = configs.Where(x => x.EditStatus != EditStatus.Deleted).ToList();
        var kvList = new List<KeyValuePair<string, string>>();
        foreach (var config in configs)
            kvList.Add(new KeyValuePair<string, string>(_configService.GenerateKey(config), config.Value));

        kvList = kvList.OrderBy(x => x.Key).ToList();
        return Json(new
        {
            success = true,
            data = kvList
        });
    }

    /// <summary>
    ///     Get configuration content in jsonc format.
    /// </summary>
    /// <param name="appId">Application ID.</param>
    /// <returns></returns>
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Read })]
    public async Task<IActionResult> GetJson(string appId, EnvString env)
    {
        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");

        var configs = await _configService.GetByAppIdAsync(appId, env.Value);
        // When producing JSON, exclude deleted configurations.
        configs = configs.Where(x => x.EditStatus != EditStatus.Deleted).ToList();
        var dict = new Dictionary<string, string>();
        var comments = new Dictionary<string, string>();
        configs.ForEach(x =>
        {
            var key = _configService.GenerateKey(x);
            dict.Add(key, x.Value);
            if (!string.IsNullOrWhiteSpace(x.Description)) comments.Add(key, x.Description);
        });

        var json = DictionaryConvertToJson.ToJsonc(dict, comments);

        return Json(new
        {
            success = true,
            data = json
        });
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Edit })]
    public async Task<IActionResult> SaveJson([FromBody] SaveJsonVM data, string appId, EnvString env)
    {
        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException(nameof(appId));

        if (data == null) throw new ArgumentNullException(nameof(data));

        if (string.IsNullOrEmpty(data.json)) throw new ArgumentNullException("data.json");

        var result = await _configService.SaveJsonAsync(data.json, appId, env.Value, data.isPatch);

        return Json(new
        {
            success = result
        });
    }

    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.Config_Edit })]
    public async Task<IActionResult> SaveKvList([FromBody] SaveKVListVM data, string appId, EnvString env)
    {
        if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException(nameof(appId));

        if (data == null) throw new ArgumentNullException(nameof(data));

        var validateResult = _configService.ValidateKvString(data.str);
        if (!validateResult.Item1)
            return Json(new
            {
                success = false,
                message = validateResult.Item2
            });

        var result = await _configService.SaveKvListAsync(data.str, appId, env.Value, data.isPatch);

        return Json(new
        {
            success = result
        });
    }
}
