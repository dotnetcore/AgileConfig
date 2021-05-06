using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class EventRegister : IEventRegister
    {
        private IAppService _appService;
        private IConfigService _configService;
        private ISysLogService _sysLogService;
        private IModifyLogService _modifyLogService;
        private IRemoteServerNodeProxy _remoteServerNodeProxy;
        private IServerNodeService _serverNodeService;

        public EventRegister(IRemoteServerNodeProxy remoteServerNodeProxy)
        {
            _appService = new AppService(new FreeSqlContext(FreeSQL.Instance));
            _configService = new ConfigService(new FreeSqlContext(FreeSQL.Instance), null, _appService);
            _sysLogService = new SysLogService(new FreeSqlContext(FreeSQL.Instance));
            _modifyLogService = new ModifyLogService(new FreeSqlContext(FreeSQL.Instance));
            _serverNodeService = new ServerNodeService(new FreeSqlContext(FreeSQL.Instance));

            _remoteServerNodeProxy = remoteServerNodeProxy;
        }

        public void Init()
        {
            RegisterAddSysLog();
        }

        /// <summary>
        /// 注册添加系统日志事件
        /// </summary>
        private void RegisterAddSysLog()
        {
            TinyEventBus.Instance.Regist(EventKeys.ADMIN_LOGIN_SUCCESS, (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"管理员登录成功"
                };
                _sysLogService.AddSysLogAsync(log);
            });

            TinyEventBus.Instance.Regist(EventKeys.INIT_ADMIN_PASSWORD_SUCCESS, (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"管理员密码初始化成功"
                };
                _sysLogService.AddSysLogAsync(log);
            });

            TinyEventBus.Instance.Regist(EventKeys.RESET_ADMIN_PASSWORD_SUCCESS, (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"修改管理员密码成功"
                };
                _sysLogService.AddSysLogAsync(log);
            });

            TinyEventBus.Instance.Regist(EventKeys.ADD_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"新增应用【AppId：{app.Id}】【AppName：{app.Name}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            // app
            TinyEventBus.Instance.Regist(EventKeys.EDIT_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"编辑应用【AppId：{app.Id}】【AppName：{app.Name}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DISABLE_OR_ENABLE_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"{(app.Enabled ? "启用" : "禁用")}应用【AppId:{app.Id}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"删除应用【AppId:{app.Id}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"删除应用【AppId:{app.Id}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });
            //config
            TinyEventBus.Instance.Regist(EventKeys.ADD_CONFIG_SUCCESS, (param) =>
            {
                var config = param as Config;
                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"新增配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    _sysLogService.AddSysLogAsync(log);

                    _modifyLogService.AddAsync(new ModifyLog
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ConfigId = config.Id,
                        Key = config.Key,
                        Group = config.Group,
                        Value = config.Value,
                        ModifyTime = config.CreateTime
                    });
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.EDIT_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                Config oldConfig = param_dy.oldConfig;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"编辑配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    _sysLogService.AddSysLogAsync(log);

                    _modifyLogService.AddAsync(new ModifyLog
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ConfigId = config.Id,
                        Key = config.Key,
                        Group = config.Group,
                        Value = config.Value,
                        ModifyTime = config.UpdateTime.Value
                    });
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.EDIT_CONFIG_SUCCESS, async (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                Config oldConfig = param_dy.oldConfig;

                if (config != null)
                {
                    if (config.OnlineStatus == OnlineStatus.Online)
                    {
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
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_CONFIG_SUCCESS, (param) =>
            {
                var config = param as Config;
                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"删除配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }

            });
            TinyEventBus.Instance.Regist(EventKeys.DELETE_CONFIG_SUCCESS, async (param) =>
            {
                var config = param as Config;
                if (config != null)
                {
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
            });

            TinyEventBus.Instance.Regist(EventKeys.OFFLINE_CONFIG_SUCCESS, (param) =>
            {
                var config = param as Config;
                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"下线配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.OFFLINE_CONFIG_SUCCESS, async (param) =>
            {
                var config = param as Config;
                if (config != null)
                {
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
            });
          

            TinyEventBus.Instance.Regist(EventKeys.PUBLISH_CONFIG_SUCCESS, (param) =>
            {
                Config config = param as Config;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"上线配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.PUBLISH_CONFIG_SUCCESS, async (param) =>
            {
                Config config = param as Config;

                if (config != null)
                {
                    if (config != null && config.OnlineStatus == OnlineStatus.Online)
                    {
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
                }

            });


            TinyEventBus.Instance.Regist(EventKeys.ROLLBACK_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                ModifyLog modifyLog = param_dy.modifyLog;

                if (config != null && modifyLog != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"回滚配置【Key:{config.Key}】 【Group：{config.Group}】 【AppId：{config.AppId}】至历史记录：{modifyLog.Id}"
                    };
                    _sysLogService.AddSysLogAsync(log);

                    _modifyLogService.AddAsync(new ModifyLog
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ConfigId = config.Id,
                        Key = config.Key,
                        Group = config.Group,
                        Value = config.Value,
                        ModifyTime = config.UpdateTime.Value
                    });
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.ROLLBACK_CONFIG_SUCCESS, async (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                Config oldConfig = param_dy.oldConfig;

                ModifyLog modifyLog = param_dy.modifyLog;

                if (config != null && oldConfig != null)
                {
                    if (config.OnlineStatus == OnlineStatus.Online)
                    {
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
                }
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
    }
}
