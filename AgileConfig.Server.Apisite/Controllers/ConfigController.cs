using System;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Agile.Config.Protocol;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using AgileConfig.Server.Common;
using System.IO;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    [ModelVaildate]
    public class ConfigController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IModifyLogService _modifyLogService;
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IServerNodeService _serverNodeService;
        private readonly ISysLogService _sysLogService;
        private readonly IAppService _appService;

        public ConfigController(
                                IConfigService configService,
                                IModifyLogService modifyLogService,
                                IRemoteServerNodeProxy remoteServerNodeProxy,
                                IServerNodeService serverNodeService,
                                ISysLogService sysLogService,
                                 IAppService appService)
        {
            _configService = configService;
            _modifyLogService = modifyLogService;
            _remoteServerNodeProxy = remoteServerNodeProxy;
            _serverNodeService = serverNodeService;
            _sysLogService = sysLogService;
            _appService = appService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConfigVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var app = await _appService.GetAsync(model.AppId);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = $"应用（{model.AppId}）不存在。"
                });
            }

            var oldConfig = await _configService.GetByAppIdKey(model.AppId, model.Group, model.Key);
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

            var result = await _configService.AddAsync(config);

            if (result)
            {
                //add syslog
                await _sysLogService.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = config.AppId,
                    LogText = $"新增配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                });
                //add modify log 
                await _modifyLogService.AddAsync(new ModifyLog
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ConfigId = config.Id,
                    Key = config.Key,
                    Group = config.Group,
                    Value = config.Value,
                    ModifyTime = config.CreateTime
                });
            }

            return Json(new
            {
                success = result,
                message = !result ? "新建配置失败，请查看错误日志" : "",
                data = config
            }) ;
        }
        [HttpPost]
        public async Task<IActionResult> AddRange([FromBody] List<ConfigVM> model)
        {
            if (model == null || model.Count == 0)
            {
                throw new ArgumentNullException("model");
            }

            var configs = await _configService.GetByAppId(model.First().AppId);

            var oldDict = new Dictionary<string, string>();
            configs.ForEach(item =>
            {
                var newkey = item.Key;
                if (!string.IsNullOrEmpty(item.Group))
                {
                    newkey = $"{item.Group}:{item.Key}";
                }
                oldDict.Add(newkey, item.Value);
            });

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
                addConfigs.Add(config);
            }

            var result = await _configService.AddRangeAsync(addConfigs);

            if (result)
            {
                //add syslogs
                var addSysLogs = new List<SysLog>();
                addConfigs.ForEach(c =>
                {
                    addSysLogs.Add(new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = c.AppId,
                        LogText = $"新增配置【Key：{c.Key}】【Value：{c.Value}】【Group：{c.Group}】【AppId：{c.AppId}】"
                    });
                });
                await _sysLogService.AddRangeAsync(addSysLogs);
                //add modify log 
                var addModifyLogs = new List<ModifyLog>();
                addConfigs.ForEach(c =>
                {
                    addModifyLogs.Add(new ModifyLog
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ConfigId = c.Id,
                        Key = c.Key,
                        Group = c.Group,
                        Value = c.Value,
                        ModifyTime = c.CreateTime
                    });
                });
                await _modifyLogService.AddRangAsync(addModifyLogs);
            }

            return Json(new
            {
                success = result,
                message = !result ? "批量新增配置失败，请查看错误日志" : ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ConfigVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var config = await _configService.GetAsync(model.Id);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }

            var app = await _configService.GetByAppId(model.AppId);
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
                var anotherConfig = await _configService.GetByAppIdKey(model.AppId, model.Group, model.Key);
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
            config.Status = model.Status;
            config.UpdateTime = DateTime.Now;

            var result = await _configService.UpdateAsync(config);

            if (result && !IsOnlyUpdateDescription(config, oldConfig))
            {
                //add modify log 
                await _modifyLogService.AddAsync(new ModifyLog
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ConfigId = config.Id,
                    Key = config.Key,
                    Group = config.Group,
                    Value = config.Value,
                    ModifyTime = config.UpdateTime.Value
                });
                //syslog
                await _sysLogService.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = config.AppId,
                    LogText = $"编辑配置【Key:{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                });
                //notice clients
                var action = new WebsocketAction
                {
                    Action = ActionConst.Update,
                    Item = new ConfigItem { group = config.Group, key = config.Key, value = config.Value },
                    OldItem = new ConfigItem { group = oldConfig.Group, key = oldConfig.Key, value = oldConfig.Value }
                };
                var nodes = await _serverNodeService.GetAllNodesAsync();
                foreach (var node in nodes)
                {
                    if (node.Status == NodeStatus.Offline)
                    {
                        continue;
                    }
                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, config.AppId, action);
                }
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
            return newConfig.Key == oldConfig.Key && newConfig.Group == oldConfig.Group && newConfig.Value == oldConfig.Value;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var configs = await _configService.GetAllConfigsAsync();

            return Json(new
            {
                success = true,
                data = configs
            });
        }

        /// <summary>
        /// 按多条件进行搜索
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Search(string appId, string group, string key, int pageSize, int pageIndex)
        {
            if (pageSize == 0)
            {
                throw new ArgumentException("pageSize can not be 0 .");
            }
            if (pageIndex == 0)
            {
                throw new ArgumentException("pageIndex can not be 0 .");
            }

            var configs = await _configService.Search(appId, group, key);
            configs = configs.Where(c => c.Status == ConfigStatus.Enabled).ToList();
            configs = configs.OrderBy(c => c.AppId).ThenBy(c => c.Group).ThenBy(c => c.Key).ToList();

            var page = configs.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var total = configs.Count();
            var totalPages = total / pageSize;
            if ((total % pageSize) > 0)
            {
                totalPages++;
            }

            return Json(new
            {
                success = true,
                data = page,
                totalPages
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var config = await _configService.GetAsync(id);

            return Json(new
            {
                success = config != null,
                data = config,
                message = config == null ? "未找到对应的配置项。" : ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var config = await _configService.GetAsync(id);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }

            config.Status = ConfigStatus.Deleted;

            var result = await _configService.UpdateAsync(config);

            if (result)
            {
                //add syslog
                await _sysLogService.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = config.AppId,
                    LogText = $"删除配置【Key:{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                });
                //notice clients
                var action = await CreateRemoveWebsocketAction(config, config.AppId);
                var nodes = await _serverNodeService.GetAllNodesAsync();
                foreach (var node in nodes)
                {
                    if (node.Status == NodeStatus.Offline)
                    {
                        continue;
                    }
                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, config.AppId, action);
                }

            }

            return Json(new
            {
                success = result,
                message = !result ? "修改配置失败，请查看错误日志" : ""
            });
        }

        private async Task<WebsocketAction> CreateRemoveWebsocketAction(Config oldConfig, string appId)
        {
            //获取app此时的配置列表合并继承的app配置 字典
            var configs = await _configService.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(appId);
            var oldKey = _configService.GenerateKey(oldConfig);
            //如果oldkey已经不存在，返回remove的action
            if (!configs.ContainsKey(oldKey))
            {
                var action = new WebsocketAction { Action = ActionConst.Remove, Item = new ConfigItem { group = oldConfig.Group, key = oldConfig.Key, value = oldConfig.Value } };
                return action;
            }
            else
            {
                //如果还在，那么说明有继承的app的配置项目的key跟oldkey一样，那么使用继承的配置的值
                //返回update的action
                var config = configs[oldKey];
                var action = new WebsocketAction
                {
                    Action = ActionConst.Update,
                    Item = new ConfigItem { group = config.Group, key = config.Key, value = config.Value },
                    OldItem = new ConfigItem { group = oldConfig.Group, key = oldConfig.Key, value = oldConfig.Value }
                };

                return action;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Rollback(string configId, string logId)
        {
            if (string.IsNullOrEmpty(configId))
            {
                throw new ArgumentNullException("configId");
            }
            if (string.IsNullOrEmpty(logId))
            {
                throw new ArgumentNullException("logId");
            }

            var config = await _configService.GetAsync(configId);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }
            var oldConfig = new Config
            {
                Key = config.Key,
                Group = config.Group,
                Value = config.Value
            };

            var log = await _modifyLogService.GetAsync(logId);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项的历史记录项。"
                });
            }
            config.Key = log.Key;
            config.Group = log.Group;
            config.Value = log.Value;
            config.UpdateTime = DateTime.Now;

            var result = await _configService.UpdateAsync(config);
            if (result)
            {
                //add modify log 
                await _modifyLogService.AddAsync(new ModifyLog
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ConfigId = config.Id,
                    Key = config.Key,
                    Group = config.Group,
                    Value = config.Value,
                    ModifyTime = config.UpdateTime.Value
                });
                //add syslog
                await _sysLogService.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = config.AppId,
                    LogText = $"回滚配置【Key:{config.Key}】 【Group：{config.Group}】 【AppId：{config.AppId}】至历史记录：{logId}"
                });
                //notice clients
                var action = new WebsocketAction
                {
                    Action = ActionConst.Update,
                    Item = new ConfigItem { group = config.Group, key = config.Key, value = config.Value },
                    OldItem = new ConfigItem { group = oldConfig.Group, key = oldConfig.Key, value = oldConfig.Value }
                };
                var nodes = await _serverNodeService.GetAllNodesAsync();
                foreach (var node in nodes)
                {
                    if (node.Status == NodeStatus.Offline)
                    {
                        continue;
                    }
                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, config.AppId, action);
                }
            }

            return Json(new
            {
                success = result,
                message = !result ? "回滚失败，请查看错误日志。" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> ModifyLogs(string configId)
        {
            if (string.IsNullOrEmpty(configId))
            {
                throw new ArgumentNullException("configId");
            }

            var logs = await _modifyLogService.Search(configId);

            return Json(new
            {
                success = true,
                data = logs.OrderByDescending(l => l.ModifyTime).ToList()
            }); ;
        }

        /// <summary>
        /// 下线
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Offline(string configId)
        {
            if (string.IsNullOrEmpty(configId))
            {
                throw new ArgumentNullException("configId");
            }

            var config = await _configService.GetAsync(configId);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }
            config.OnlineStatus = OnlineStatus.WaitPublish;
            var result = await _configService.UpdateAsync(config);
            if (result)
            {
                await _sysLogService.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = config.AppId,
                    LogText = $"下线配置【Key:{config.Key}】 【Group：{config.Group}】 【AppId：{config.AppId}】"
                });
                //notice clients the config item is offline
                var action = new WebsocketAction { Action = ActionConst.Remove, Item = new ConfigItem { group = config.Group, key = config.Key, value = config.Value } };
                var nodes = await _serverNodeService.GetAllNodesAsync();
                foreach (var node in nodes)
                {
                    if (node.Status == NodeStatus.Offline)
                    {
                        continue;
                    }
                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, config.AppId, action);
                }
            }

            return Json(new
            {
                success = result,
                message = !result ? "下线配置失败，请查看错误日志" : ""
            });
        }


        /// <summary>
        /// 上线多个配置
        /// </summary>
        /// <param name="configIds"></param>
        /// <returns></returns>
        public async Task<IActionResult> PublishSome([FromBody] List<string> configIds)
        {
            if (configIds == null)
            {
                throw new ArgumentNullException("configIds");
            }

            var nodes = await _serverNodeService.GetAllNodesAsync();
            foreach (var configId in configIds)
            {
                var config = await _configService.GetAsync(configId);
                if (config == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "未找到对应的配置项。"
                    });
                }
                if (config.OnlineStatus == OnlineStatus.Online)
                {
                    continue;
                }
                config.OnlineStatus = OnlineStatus.Online;
                var result = await _configService.UpdateAsync(config);
                if (result)
                {
                    await _sysLogService.AddSysLogAsync(new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"上线配置【Key:{config.Key}】 【Group：{config.Group}】 【AppId：{config.AppId}】"
                    });
                    //notice clients config item is published
                    var action = new WebsocketAction
                    {
                        Action = ActionConst.Add,
                        Item = new ConfigItem { group = config.Group, key = config.Key, value = config.Value }
                    };
                    foreach (var node in nodes)
                    {
                        if (node.Status == NodeStatus.Offline)
                        {
                            continue;
                        }
                        await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, config.AppId, action);
                    }
                }
            }
            return Json(new
            {
                success = true,
                message = "上线配置成功"
            });
        }

        /// <summary>
        /// 上线1个配置
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Publish(string configId)
        {
            if (string.IsNullOrEmpty(configId))
            {
                throw new ArgumentNullException("configId");
            }

            var config = await _configService.GetAsync(configId);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }

            if (config.OnlineStatus == OnlineStatus.Online)
            {
                return Json(new
                {
                    success = false,
                    message = "该配置已上线"
                });
            }

            config.OnlineStatus = OnlineStatus.Online;
            var result = await _configService.UpdateAsync(config);
            if (result)
            {
                await _sysLogService.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = config.AppId,
                    LogText = $"上线配置【Key:{config.Key}】 【Group：{config.Group}】 【AppId：{config.AppId}】"
                });
                //notice clients config item is published
                var action = new WebsocketAction
                {
                    Action = ActionConst.Add,
                    Item = new ConfigItem { group = config.Group, key = config.Key, value = config.Value }
                };
                var nodes = await _serverNodeService.GetAllNodesAsync();
                foreach (var node in nodes)
                {
                    if (node.Status == NodeStatus.Offline)
                    {
                        continue;
                    }
                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, config.AppId, action);
                }
            }
            return Json(new
            {
                success = result,
                message = !result ? "上线配置失败，请查看错误日志" : ""
            });
        }

        public IActionResult PreViewJsonFile(List<IFormFile> files)
        {
            if (files == null || !files.Any())
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
                //judge if json key already in configs
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
                    addConfigs.Add(config);
                }

                //var result = await _configService.AddRangeAsync(configs);

                return Json(new
                {
                    success = true,
                    data = addConfigs
                });
            }
        }
    }
}
