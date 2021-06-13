using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq;


namespace AgileConfig.Server.Service
{
    public class EventRegister : IEventRegister
    {
        private IAppService GetAppService()
        {
            return new AppService(new FreeSqlContext(FreeSQL.Instance));
        }
        private IConfigService GetConfigService()
        {
            return new ConfigService(new FreeSqlContext(FreeSQL.Instance), null, GetAppService());
        }

        private ISysLogService GetSysLogService() 
        {
            return new SysLogService(new FreeSqlContext(FreeSQL.Instance));
        }
        private IModifyLogService GetModifyLogService() 
        {
            return new ModifyLogService(new FreeSqlContext(FreeSQL.Instance));
        }
        private IServerNodeService GetServerNodeService()
        {
            return new ServerNodeService(new FreeSqlContext(FreeSQL.Instance)); 
        }

        private IRemoteServerNodeProxy _remoteServerNodeProxy;
        public EventRegister(IRemoteServerNodeProxy remoteServerNodeProxy)
        {
            _remoteServerNodeProxy = remoteServerNodeProxy;
        }

        public void Init()
        {
            RegisterAddSysLog();
            RegisterWebsocketAction();
        }

        private void RegisterWebsocketAction()
        {
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
                        using (var serverNodeService = GetServerNodeService())
                        {
                            var nodes = await serverNodeService.GetAllNodesAsync();
                            var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(config);
                            noticeApps.Add(config.AppId, action);

                            foreach (var node in nodes)
                            {
                                if (node.Status == NodeStatus.Offline)
                                {
                                    continue;
                                }
                                foreach (var kv in noticeApps)
                                {
                                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, kv.Key, kv.Value);
                                }
                            }
                        }
                    }
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.DELETE_CONFIG_SUCCESS, async (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                Config oldConfig = param_dy.oldConfig;
                if (config != null)
                {
                    var action = await CreateRemoveWebsocketAction(config, config.AppId);
                    using (var serverNodeService = GetServerNodeService())
                    {
                        var nodes = await serverNodeService.GetAllNodesAsync();
                        var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(config);
                        noticeApps.Add(config.AppId, await CreateRemoveWebsocketAction(oldConfig, config.AppId));

                        foreach (var node in nodes)
                        {
                            if (node.Status == NodeStatus.Offline)
                            {
                                continue;
                            }
                            foreach (var kv in noticeApps)
                            {
                                await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, kv.Key, kv.Value);
                            }
                        }
                    }
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.OFFLINE_CONFIG_SUCCESS, async (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                Config oldConfig = param_dy.oldConfig;
                if (config != null)
                {
                    //notice clients the config item is offline
                    using (var serverNodeService = GetServerNodeService())
                    {
                        var nodes = await serverNodeService.GetAllNodesAsync();
                        var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(config);
                        noticeApps.Add(config.AppId, await CreateRemoveWebsocketAction(oldConfig, config.AppId));

                        foreach (var node in nodes)
                        {
                            if (node.Status == NodeStatus.Offline)
                            {
                                continue;
                            }
                            foreach (var kv in noticeApps)
                            {
                                await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, kv.Key, kv.Value);
                            }
                        }
                    }
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.PUBLISH_CONFIG_SUCCESS, async (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;

                if (config != null)
                {
                    if (config.OnlineStatus == OnlineStatus.Online)
                    {
                        //notice clients config item is published
                        var action = new WebsocketAction
                        {
                            Action = ActionConst.Add,
                            Item = new ConfigItem { group = config.Group, key = config.Key, value = config.Value }
                        };
                        using (var serverNodeService = GetServerNodeService())
                        {
                            var nodes = await serverNodeService.GetAllNodesAsync();
                            var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(config);
                            noticeApps.Add(config.AppId, action);

                            foreach (var node in nodes)
                            {
                                if (node.Status == NodeStatus.Offline)
                                {
                                    continue;
                                }
                                foreach (var item in noticeApps)
                                {
                                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, item.Key, item.Value);
                                }
                            }
                        }
                    }
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
                        using (var serverNodeService = GetServerNodeService())
                        {
                            var nodes = await serverNodeService.GetAllNodesAsync();
                            var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(config);
                            noticeApps.Add(config.AppId, action);

                            foreach (var node in nodes)
                            {
                                if (node.Status == NodeStatus.Offline)
                                {
                                    continue;
                                }
                                foreach (var item in noticeApps)
                                {
                                    await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, item.Key, item.Value);
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 注册添加系统日志事件
        /// </summary>
        private void RegisterAddSysLog()
        {
            TinyEventBus.Instance.Regist(EventKeys.USER_LOGIN_SUCCESS, (param) =>
            {
                dynamic param_dy = param as dynamic;
                string userName = param_dy.userName;
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"{userName} 登录成功"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.INIT_SUPERADMIN_PASSWORD_SUCCESS, (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"超级管理员密码初始化成功"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.RESET_USER_PASSWORD_SUCCESS, (param) =>
            {
                dynamic param_dy = param as dynamic;
                User user = param_dy.user;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户 {userName} 重置 {user.UserName} 的密码为默认密码 "
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.CHANGE_USER_PASSWORD_SUCCESS, (param) =>
            {
                dynamic param_dy = param as dynamic;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"修改用户 {userName} 的密码成功"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.ADD_APP_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                App app = param_dy.app;
                string userName = param_dy.userName;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"用户：{userName} 新增应用【AppId：{app.Id}】【AppName：{app.Name}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }
                }
            });

            // app
            TinyEventBus.Instance.Regist(EventKeys.EDIT_APP_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                App app = param_dy.app;
                string userName = param_dy.userName;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"用户：{userName} 编辑应用【AppId：{app.Id}】【AppName：{app.Name}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DISABLE_OR_ENABLE_APP_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                App app = param_dy.app;
                string userName = param_dy.userName;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"用户：{userName} {(app.Enabled ? "启用" : "禁用")}应用【AppId:{app.Id}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_APP_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                App app = param_dy.app;
                string userName = param_dy.userName;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"用户：{userName} 删除应用【AppId:{app.Id}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }
                }
            });

            //config
            TinyEventBus.Instance.Regist(EventKeys.ADD_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                string userName = param_dy.userName;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"用户：{userName} 新增配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }

                    using (var modifyLogService = GetModifyLogService())
                    {
                        modifyLogService.AddAsync(new ModifyLog
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            ConfigId = config.Id,
                            Key = config.Key,
                            Group = config.Group,
                            Value = config.Value,
                            ModifyTime = config.CreateTime
                        });
                    }
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.EDIT_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                Config oldConfig = param_dy.oldConfig;
                string userName = param_dy.userName;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"用户：{userName} 编辑配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }

                    using (var modifyLogService = GetModifyLogService())
                    {
                        modifyLogService.AddAsync(new ModifyLog
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            ConfigId = config.Id,
                            Key = config.Key,
                            Group = config.Group,
                            Value = config.Value,
                            ModifyTime = config.UpdateTime.Value
                        });
                    }
                }
            });

         

            TinyEventBus.Instance.Regist(EventKeys.DELETE_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                string userName = param_dy.userName;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"用户：{userName} 删除配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }
                }

            });
        

            TinyEventBus.Instance.Regist(EventKeys.OFFLINE_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                string userName = param_dy.userName;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"用户：{userName} 下线配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }
                }
            });
       




            TinyEventBus.Instance.Regist(EventKeys.PUBLISH_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                string userName = param_dy.userName;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"用户：{userName} 上线配置【Key：{config.Key}】【Value：{config.Value}】【Group：{config.Group}】【AppId：{config.AppId}】"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }
                }
            });
           


            TinyEventBus.Instance.Regist(EventKeys.ROLLBACK_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                Config config = param_dy.config;
                ModifyLog modifyLog = param_dy.modifyLog;
                string userName = param_dy.userName;

                if (config != null && modifyLog != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"用户：{userName} 回滚配置【Key:{config.Key}】 【Group：{config.Group}】 【AppId：{config.AppId}】至历史记录：{modifyLog.Id}"
                    };
                    using (var syslogService = GetSysLogService())
                    {
                        syslogService.AddSysLogAsync(log);
                    }

                    using (var modifyLogService = GetModifyLogService())
                    {
                        modifyLogService.AddAsync(new ModifyLog
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            ConfigId = config.Id,
                            Key = config.Key,
                            Group = config.Group,
                            Value = config.Value,
                            ModifyTime = config.UpdateTime.Value
                        });
                    }
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.ADD_NODE_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                ServerNode node = param_dy.node;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 添加节点：{node.Address}"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_NODE_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                ServerNode node = param_dy.node;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 删除节点：{node.Address}"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.ADD_USER_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                User user = param_dy.user;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 添加用户：{user.UserName} 成功"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.EDIT_USER_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                User user = param_dy.user;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 编辑用户：{user.UserName} 成功"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_USER_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                User user = param_dy.user;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 删除用户：{user.UserName} 成功"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DISCONNECT_CLIENT_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                string clientId = param_dy.clientId;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 断开客户端 {clientId} 成功"
                };
                using (var syslogService = GetSysLogService())
                {
                    syslogService.AddSysLogAsync(log);
                }
            });
        }

        /// <summary>
        /// 根据当前配置计算需要通知的应用
        /// </summary>
        /// <param name="currentUpdateConfig"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, WebsocketAction>> GetNeedNoticeInheritancedFromAppsAction(Config config)
        {
            Dictionary<string, WebsocketAction> needNoticeAppsActions = new Dictionary<string, WebsocketAction>
            {
            };
            using (var appService = GetAppService())
            {
                var currentApp = await appService.GetAsync(config.AppId);
                if (currentApp.Type == AppType.Inheritance)
                {
                    var inheritancedFromApps = await appService.GetInheritancedFromAppsAsync(config.AppId);
                    inheritancedFromApps.ForEach(x =>
                    {
                        needNoticeAppsActions.Add(x.Id, new WebsocketAction
                        {
                            Action = ActionConst.Reload
                        });
                    });
                }

                return needNoticeAppsActions;
            }
          
        }

        private async Task<WebsocketAction> CreateRemoveWebsocketAction(Config oldConfig, string appId)
        {
            using (var configService = GetConfigService())
            {
                //获取app此时的配置列表合并继承的app配置 字典
                var configs = await configService.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(appId);
                var oldKey = configService.GenerateKey(oldConfig);
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
}
