using System;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using AgileConfig.Server.Common;
using System.Text;
using System.Dynamic;
using System.IO;
using AgileConfig.Server.Apisite.Utilites;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    [ModelVaildate]
    public class ConfigController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IAppService _appService;
        private readonly IUserService _userService;

        public ConfigController(
            IConfigService configService,
            IAppService appService,
            IUserService userService)
        {
            _configService = configService;
            _appService = appService;
            _userService = userService;
        }

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "Config.Add", Functions.Config_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConfigVM model, [FromQuery] string env)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var app = await _appService.GetAsync(model.AppId);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = $"应用（{model.AppId}）不存在。"
                });
            }

            var oldConfig = await _configService.GetByAppIdKeyEnv(model.AppId, model.Group, model.Key, env);
            if (oldConfig != null)
            {
                return Json(new
                {
                    success = false,
                    message = "配置已存在，请更改输入的信息。"
                });
            }

            var config = new Config();
            config.Id = string.IsNullOrEmpty(config.Id) ? Guid.NewGuid().ToString("N") : config.Id;
            config.Key = model.Key;
            config.AppId = model.AppId;
            config.Description = model.Description;
            config.Value = model.Value;
            config.Group = model.Group;
            config.Status = ConfigStatus.Enabled;
            config.CreateTime = DateTime.Now;
            config.UpdateTime = null;
            config.OnlineStatus = OnlineStatus.WaitPublish;
            config.EditStatus = EditStatus.Add;
            config.Env = env;

            var result = await _configService.AddAsync(config, env);

            if (result)
            {
                dynamic param = new ExpandoObject();
                param.config = config;
                param.userName = this.GetCurrentUserName();
                TinyEventBus.Instance.Fire(EventKeys.ADD_CONFIG_SUCCESS, param);
            }

            return Json(new
            {
                success = result,
                message = !result ? "新建配置失败，请查看错误日志" : "",
                data = config
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute),
            Arguments = new object[] { "Config.AddRange", Functions.Config_Add })]
        [HttpPost]
        public async Task<IActionResult> AddRange([FromBody] List<ConfigVM> model, [FromQuery] string env)
        {
            if (model == null || model.Count == 0)
            {
                throw new ArgumentNullException("model");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.GetByAppIdAsync(model.First().AppId, env);

            var oldDict = new Dictionary<string, string>();
            configs.ForEach(item => { oldDict.Add(_configService.GenerateKey(item), item.Value); });

            var addConfigs = new List<Config>();
            //judge if json key already in configs
            foreach (var item in model)
            {
                var newkey = item.Key;
                if (!string.IsNullOrEmpty(item.Group))
                {
                    newkey = $"{item.Group}:{item.Key}";
                }

                if (oldDict.ContainsKey(newkey))
                {
                    return Json(new
                    {
                        success = false,
                        message = "存在重复的配置：" + item.Key
                    });
                }

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
                config.Env = env;

                addConfigs.Add(config);
            }

            var result = await _configService.AddRangeAsync(addConfigs, env);

            if (result)
            {
                var userName = this.GetCurrentUserName();
                addConfigs.ForEach(c =>
                {
                    dynamic param = new ExpandoObject();
                    param.config = c;
                    param.userName = userName;
                    TinyEventBus.Instance.Fire(EventKeys.ADD_CONFIG_SUCCESS, param);
                });
            }

            return Json(new
            {
                success = result,
                message = !result ? "批量新增配置失败，请查看错误日志" : ""
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute),
            Arguments = new object[] { "Config.Edit", Functions.Config_Edit })]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ConfigVM model, [FromQuery] string env)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var config = await _configService.GetAsync(model.Id, env);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }

            var app = await _configService.GetByAppIdAsync(model.AppId, env);
            if (!app.Any())
            {
                return Json(new
                {
                    success = false,
                    message = $"应用（{model.AppId}）不存在。"
                });
            }

            var oldConfig = new Config
            {
                Key = config.Key,
                Group = config.Group,
                Value = config.Value
            };
            if (config.Group != model.Group || config.Key != model.Key)
            {
                var anotherConfig = await _configService.GetByAppIdKeyEnv(model.AppId, model.Group, model.Key, env);
                if (anotherConfig != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "配置键已存在，请重新输入。"
                    });
                }
            }

            config.AppId = model.AppId;
            config.Description = model.Description;
            config.Key = model.Key;
            config.Value = model.Value;
            config.Group = model.Group;
            config.UpdateTime = DateTime.Now;
            config.Env = env;

            if (!IsOnlyUpdateDescription(config, oldConfig))
            {
                var isPublished = await _configService.IsPublishedAsync(config.Id, env);
                if (isPublished)
                {
                    //如果是已发布的配置，修改后状态设置为编辑
                    config.EditStatus = EditStatus.Edit;
                }
                else
                {
                    //如果没有发布，说明是新增的，一直维持新增状态
                    config.EditStatus = EditStatus.Add;
                }

                config.OnlineStatus = OnlineStatus.WaitPublish;
            }

            var result = await _configService.UpdateAsync(config, env);

            if (result)
            {
                dynamic param = new ExpandoObject();
                param.config = config;
                param.userName = this.GetCurrentUserName();
                param.oldConfig = config;
                TinyEventBus.Instance.Fire(EventKeys.EDIT_CONFIG_SUCCESS, param);
            }

            return Json(new
            {
                success = result,
                message = !result ? "修改配置失败，请查看错误日志。" : ""
            });
        }

        /// <summary>
        /// 是否只是修改了描述信息
        /// </summary>
        /// <param name="newConfig"></param>
        /// <param name="oldConfig"></param>
        /// <returns></returns>
        private bool IsOnlyUpdateDescription(Config newConfig, Config oldConfig)
        {
            return newConfig.Key == oldConfig.Key && newConfig.Group == oldConfig.Group &&
                   newConfig.Value == oldConfig.Value;
        }

        [HttpGet]
        public async Task<IActionResult> All(string env)
        {
            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.GetAllConfigsAsync(env);

            return Json(new
            {
                success = true,
                data = configs
            });
        }

        /// <summary>
        /// 按多条件进行搜索
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="group">分组</param>
        /// <param name="key">键</param>
        /// <param name="onlineStatus">在线状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="current">当前页</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Search(string appId, string group, string key, OnlineStatus? onlineStatus,
            string sortField, string ascOrDesc, string env, int pageSize = 20, int current = 1)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentException("pageSize can not less then 1 .");
            }

            if (current <= 0)
            {
                throw new ArgumentException("pageIndex can not less then 1 .");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.Search(appId, group, key, env);
            configs = configs.Where(c => c.Status == ConfigStatus.Enabled).ToList();
            if (onlineStatus.HasValue)
            {
                configs = configs.Where(c => c.OnlineStatus == onlineStatus).ToList();
            }

            if (sortField == "createTime")
            {
                if (ascOrDesc.StartsWith("asc"))
                {
                    configs = configs.OrderBy(x => x.CreateTime).ToList();
                }
                else
                {
                    configs = configs.OrderByDescending(x => x.CreateTime).ToList();
                }
            }

            if (sortField == "group")
            {
                if (ascOrDesc.StartsWith("asc"))
                {
                    configs = configs.OrderBy(x => x.Group).ToList();
                }
                else
                {
                    configs = configs.OrderByDescending(x => x.Group).ToList();
                }
            }

            var page = configs.Skip((current - 1) * pageSize).Take(pageSize).ToList();
            var total = configs.Count();

            return Json(new
            {
                current,
                pageSize,
                success = true,
                total = total,
                data = page
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id, string env)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var config = await _configService.GetAsync(id, env);

            return Json(new
            {
                success = config != null,
                data = config,
                message = config == null ? "未找到对应的配置项。" : ""
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute),
            Arguments = new object[] { "Config.Delete", Functions.Config_Delete })]
        [HttpPost]
        public async Task<IActionResult> Delete(string id, string env)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var config = await _configService.GetAsync(id, env);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }

            config.EditStatus = EditStatus.Deleted;
            config.OnlineStatus = OnlineStatus.WaitPublish;

            var isPublished = await _configService.IsPublishedAsync(config.Id, env);
            if (!isPublished)
            {
                //如果已经没有发布过直接删掉
                config.Status = ConfigStatus.Deleted;
            }

            var result = await _configService.UpdateAsync(config, env);
            if (result)
            {
                dynamic param = new ExpandoObject();
                param.config = config;
                param.userName = this.GetCurrentUserName();
                TinyEventBus.Instance.Fire(EventKeys.DELETE_CONFIG_SUCCESS, param);
            }

            return Json(new
            {
                success = result,
                message = !result ? "删除配置失败，请查看错误日志" : ""
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute),
            Arguments = new object[] { "Config.DeleteSome", Functions.Config_Delete })]
        [HttpPost]
        public async Task<IActionResult> DeleteSome([FromBody] List<string> ids, string env)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            List<Config> deleteConfigs = new List<Config>();

            foreach (var id in ids)
            {
                var config = await _configService.GetAsync(id, env);
                if (config == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "未找到对应的配置项。"
                    });
                }

                config.EditStatus = EditStatus.Deleted;
                config.OnlineStatus = OnlineStatus.WaitPublish;

                var isPublished = await _configService.IsPublishedAsync(config.Id, env);
                if (!isPublished)
                {
                    //如果已经没有发布过直接删掉
                    config.Status = ConfigStatus.Deleted;
                }

                deleteConfigs.Add(config);
            }

            var result = await _configService.UpdateAsync(deleteConfigs, env);
            if (result)
            {
                dynamic param = new ExpandoObject();
                param.userName = this.GetCurrentUserName();
                param.appId = deleteConfigs.First().AppId;
                param.env = env;
                TinyEventBus.Instance.Fire(EventKeys.DELETE_CONFIG_SOME_SUCCESS, param);
            }

            return Json(new
            {
                success = result,
                message = !result ? "删除配置失败，请查看错误日志" : ""
            });
        }


        [TypeFilter(typeof(PremissionCheckAttribute),
            Arguments = new object[] { "Config.Rollback", Functions.Config_Publish })]
        [HttpPost]
        public async Task<IActionResult> Rollback(string publishTimelineId, string env)
        {
            if (string.IsNullOrEmpty(publishTimelineId))
            {
                throw new ArgumentNullException("publishTimelineId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var result = await _configService.RollbackAsync(publishTimelineId, env);

            if (result)
            {
                dynamic param = new ExpandoObject();
                param.userName = this.GetCurrentUserName();
                param.timelineNode = await _configService.GetPublishTimeLineNodeAsync(publishTimelineId, env);
                param.env = env;
                TinyEventBus.Instance.Fire(EventKeys.ROLLBACK_CONFIG_SUCCESS, param);
            }

            return Json(new
            {
                success = result,
                message = !result ? "回滚失败，请查看错误日志。" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> ConfigPublishedHistory(string configId, string env)
        {
            if (string.IsNullOrEmpty(configId))
            {
                throw new ArgumentNullException("configId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configPublishedHistory = await _configService.GetConfigPublishedHistory(configId, env);
            var result = new List<object>();

            foreach (var publishDetail in configPublishedHistory.OrderByDescending(x => x.Version))
            {
                var timelineNode =
                    await _configService.GetPublishTimeLineNodeAsync(publishDetail.PublishTimelineId, env);
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
        /// 发布所有待发布的配置项
        /// </summary>
        /// <returns></returns>
        [TypeFilter(typeof(PremissionCheckAttribute),
            Arguments = new object[] { "Config.Publish", Functions.Config_Publish })]
        [HttpPost]
        public async Task<IActionResult> Publish([FromBody] PublishLogVM model, string env)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (string.IsNullOrEmpty(model.AppId))
            {
                throw new ArgumentNullException("appId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var appId = model.AppId;
            var userId = await this.GetCurrentUserId(_userService);
            var ret = _configService.Publish(appId, model.Log, userId, env);

            if (ret.result)
            {
                var timelineNode = await _configService.GetPublishTimeLineNodeAsync(ret.publishTimelineId, env);
                dynamic param = new ExpandoObject();
                param.publishTimelineNode = timelineNode;
                param.userName = this.GetCurrentUserName();
                param.env = env;
                TinyEventBus.Instance.Fire(EventKeys.PUBLISH_CONFIG_SUCCESS, param);
            }

            return Json(new
            {
                success = ret.result,
                message = !ret.result ? "上线配置失败，请查看错误日志" : ""
            });
        }

        /// <summary>
        /// 预览上传的json文件
        /// </summary>
        /// <returns></returns>
        public IActionResult PreViewJsonFile()
        {
            List<IFormFile> files = Request.Form.Files.ToList();
            if (!files.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "请上传Json文件"
                });
            }

            var jsonFile = files.First();
            using (var stream = jsonFile.OpenReadStream())
            {
                var dict = JsonConfigurationFileParser.Parse(stream);

                var addConfigs = new List<Config>();
                foreach (var key in dict.Keys)
                {
                    var newKey = key;
                    var group = "";
                    var paths = key.Split(":");
                    if (paths.Length > 1)
                    {
                        //如果是复杂key，取最后一个为真正的key，其他作为group
                        newKey = paths[paths.Length - 1];
                        group = string.Join(":", paths.ToList().Take(paths.Length - 1));
                    }

                    var config = new Config();
                    config.Key = newKey;
                    config.Description = "";
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

        /// <summary>
        /// 导出json文件
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <returns></returns>
        public async Task<IActionResult> ExportJson(string appId, string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.GetByAppIdAsync(appId, env);

            var dict = new Dictionary<string, string>();
            configs.ForEach(x =>
            {
                var key = _configService.GenerateKey(x);
                dict.Add(key, x.Value);
            });

            var json = DictionaryConvertToJson.ToJson(dict);

            return File(Encoding.UTF8.GetBytes(json), "application/json", $"{appId}.json");
        }

        /// <summary>
        /// 获取待发布的明细
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <returns></returns>
        public async Task<IActionResult> WaitPublishStatus(string appId, string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.Search(appId, "", "", env);
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
        /// 获取发布详情的历史
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<IActionResult> PublishHistory(string appId, string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var history = await _configService.GetPublishDetailListAsync(appId, env);

            var result = new List<object>();
            foreach (var publishDetails in history.GroupBy(x => x.Version).OrderByDescending(g => g.Key))
            {
                var data = publishDetails.ToList();
                result.Add(new
                {
                    key = publishDetails.Key,
                    timelineNode =
                        await _configService.GetPublishTimeLineNodeAsync(data.FirstOrDefault()?.PublishTimelineId, env),
                    list = data
                });
            }

            return Json(new
            {
                success = true,
                data = result
            });
        }

        public async Task<IActionResult> CancelEdit(string configId, string env)
        {
            if (string.IsNullOrEmpty(configId))
            {
                throw new ArgumentNullException("configId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var result = await _configService.CancelEdit(new List<string>() { configId }, env);

            if (result)
            {
                dynamic param = new ExpandoObject();
                param.config = await _configService.GetAsync(configId, env);
                param.userName = this.GetCurrentUserName();
                param.env = env;
                TinyEventBus.Instance.Fire(EventKeys.CANCEL_EDIT_CONFIG_SUCCESS, param);
            }

            return Json(new
            {
                success = true
            });
        }

        public async Task<IActionResult> CancelSomeEdit([FromBody] List<string> ids, string env)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var result = await _configService.CancelEdit(ids, env);

            if (result)
            {
                var config = await _configService.GetAsync(ids.First(), env);
                dynamic param = new ExpandoObject();
                param.userName = this.GetCurrentUserName();
                param.appId = config.AppId;
                param.env = env;
                TinyEventBus.Instance.Fire(EventKeys.CANCEL_EDIT_CONFIG_SOME_SUCCESS, param);
            }

            return Json(new
            {
                success = true
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute),
            Arguments = new object[] { "Config.EvnSync", Functions.Config_Add })]
        [HttpPost]
        public async Task<IActionResult> SyncEnv([FromBody] List<string> toEnvs, [FromQuery] string appId,
            [FromQuery] string currentEnv)
        {
            if (toEnvs == null)
            {
                throw new ArgumentNullException("toEnvs");
            }

            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }

            if (string.IsNullOrEmpty(currentEnv))
            {
                throw new ArgumentNullException("currentEnv");
            }

            var app = await _appService.GetAsync(appId);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = $"应用（{appId}）不存在。"
                });
            }

            var result = await _configService.EnvSync(appId, currentEnv, toEnvs);

            return Json(new
            {
                success = result
            });
        }

        public async Task<IActionResult> GetKvList(string appId, string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.GetByAppIdAsync(appId, env);
            // text 格式展示的时候不需要删除的配置
            configs = configs.Where(x => x.EditStatus != EditStatus.Deleted).ToList();
            var kvList = new List<KeyValuePair<string, string>>();
            foreach (var config in configs)
            {
                kvList.Add(new KeyValuePair<string, string>(_configService.GenerateKey(config), config.Value));
            }

            kvList = kvList.OrderBy(x => x.Key).ToList();
            return Json(new
            {
                success = true,
                data = kvList
            });
        }

        /// <summary>
        /// 获取json格式的配置
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <returns></returns>
        public async Task<IActionResult> GetJson(string appId, string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }

            env = await _configService.IfEnvEmptySetDefaultAsync(env);

            var configs = await _configService.GetByAppIdAsync(appId, env);
            // json 格式展示的时候不需要删除的配置
            configs = configs.Where(x => x.EditStatus != EditStatus.Deleted).ToList();
            var dict = new Dictionary<string, string>();
            configs.ForEach(x =>
            {
                var key = _configService.GenerateKey(x);
                dict.Add(key, x.Value);
            });

            var json = DictionaryConvertToJson.ToJson(dict);

            return Json(new
            {
                success = true,
                data = json
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveJson([FromBody] SaveJsonVM data, string appId, string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrEmpty(data.json))
            {
                throw new ArgumentNullException("data.json");
            }

            var result = await _configService.SaveJsonAsync(data.json, appId, env);

            return Json(new
            {
                success = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveKvList([FromBody] SaveKVListVM data, string appId, string env)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var validateResult = _configService.ValidateKvString(data.str);
            if (!validateResult.Item1)
            {
                return Json(new
                {
                    success = false,
                    message = validateResult.Item2
                });
            }

            var result = await _configService.SaveKvListAsync(data.str, appId, env);

            return Json(new
            {
                success = result
            });
        }
    }
}