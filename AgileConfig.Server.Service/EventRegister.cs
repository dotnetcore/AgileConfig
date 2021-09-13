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
using Microsoft.EntityFrameworkCore;


namespace AgileConfig.Server.Service
{
    public class EventRegister : IEventRegister
    {
        private IAppService GetAppService()
        {
            return new AppService(new FreeSqlContext(FreeSQL.Instance));
        }
        private IConfigService NewConfigService()
        {
            return new ConfigService(new FreeSqlContext(FreeSQL.Instance), null, GetAppService());
        }

        private ISysLogService NewSysLogService() 
        {
            return new SysLogService(new FreeSqlContext(FreeSQL.Instance));
        }
        private IServerNodeService NewServerNodeService()
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
            TinyEventBus.Instance.Regist(EventKeys.PUBLISH_CONFIG_SUCCESS,  (param) =>
            {
                dynamic param_dy = param;
                PublishTimeline timelineNode = param_dy.publishTimelineNode;
                if (timelineNode != null)
                {
                    Task.Run(async () =>
                    {
                        using (var configService = NewConfigService())
                        {
                            using (var serverNodeService = NewServerNodeService())
                            {
                                var nodes = await serverNodeService.GetAllNodesAsync();
                                var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(timelineNode.AppId);
                                noticeApps.Add(timelineNode.AppId, new WebsocketAction { Action = ActionConst.Reload });

                                foreach (var node in nodes)
                                {
                                    if (node.Status == NodeStatus.Offline)
                                    {
                                        continue;
                                    }

                                    foreach (var item in noticeApps)
                                    {
                                        await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, item.Key,
                                            item.Value);
                                    }
                                }
                            }
                        }
                    });
                }

            });

            TinyEventBus.Instance.Regist(EventKeys.ROLLBACK_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                PublishTimeline timelineNode = param_dy.timelineNode;
                if (timelineNode != null)
                {
                    Task.Run(async () =>
                    {
                        using (var configService = NewConfigService())
                        {
                            using (var serverNodeService = NewServerNodeService())
                            {
                                var nodes = await serverNodeService.GetAllNodesAsync();
                                var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(timelineNode.AppId);
                                noticeApps.Add(timelineNode.AppId, new WebsocketAction { Action = ActionConst.Reload });

                                foreach (var node in nodes)
                                {
                                    if (node.Status == NodeStatus.Offline)
                                    {
                                        continue;
                                    }

                                    foreach (var item in noticeApps)
                                    {
                                        await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, item.Key,
                                            item.Value);
                                    }
                                }
                            }
                        }
                    });
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
                Task.Run(async () =>
                {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
            });

            TinyEventBus.Instance.Regist(EventKeys.INIT_SUPERADMIN_PASSWORD_SUCCESS,  (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"超级管理员密码初始化成功"
                };
                Task.Run(async () =>
                {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
            });

            TinyEventBus.Instance.Regist(EventKeys.RESET_USER_PASSWORD_SUCCESS,  (param) =>
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
                Task.Run(async () =>
                {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
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
                Task.Run(async () =>
                {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
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
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
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
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
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
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
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
                        LogType = SysLogType.Warn,
                        LogText = $"用户：{userName} 删除应用【AppId:{app.Id}】"
                    };
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
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
                        LogText = $"用户：{userName} 新增配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】【待发布】"
                    };
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.EDIT_CONFIG_SUCCESS, (param) =>
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
                        LogText = $"用户：{userName} 编辑配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】【待发布】"
                    };
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
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
                        LogType = SysLogType.Warn,
                        AppId = config.AppId,
                        LogText = $"用户：{userName} 删除配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】【待发布】"
                    };
                    Task.Run(async ()=> {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
                }

            });
            TinyEventBus.Instance.Regist(EventKeys.DELETE_CONFIG_SOME_SUCCESS,  (param) =>
            {
                dynamic param_dy = param;
                string userName = param_dy.userName;
                string appId = param_dy.appId;
                if (appId != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Warn,
                        AppId = appId,
                        LogText = $"用户：{userName} 批量删除配置"
                    };
                    Task.Run(async ()=> {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
                }

            });

            TinyEventBus.Instance.Regist(EventKeys.PUBLISH_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                PublishTimeline node = param_dy.publishTimelineNode;
                string userName = param_dy.userName;
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = node.AppId,
                    LogText = $"用户：{userName} 发布配置【AppId：{node.AppId}】【版本：{node.PublishTime.Value:yyyyMMddHHmmss}】"
                };
                Task.Run(async ()=> {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                    using (var configService = NewConfigService())
                    {
                        var publishDetail = await configService.GetPublishDetailByPublishTimelineIdAsync(node.Id);
                    }
                });
                
            });
            TinyEventBus.Instance.Regist(EventKeys.ROLLBACK_CONFIG_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                string userName = param_dy.userName;
                PublishTimeline timelineNode = param_dy.timelineNode;

                if (timelineNode != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Warn,
                        AppId = timelineNode.AppId,
                        LogText = $"{userName} 回滚应用【{timelineNode.AppId}】至发布版本【{timelineNode.PublishTime.Value:yyyyMMddHHmmss}】"
                    };
                    Task.Run(async () => {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.CANCEL_EDIT_CONFIG_SUCCESS,  (param) =>
            {
                dynamic param_dy = param;
                string userName = param_dy.userName;
                Config config = param_dy.config;

                if (config != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = config.AppId,
                        LogText = $"{userName} 撤销编辑状态的配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】"
                    };
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.CANCEL_EDIT_CONFIG_SOME_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                string userName = param_dy.userName;
                string appId = param_dy.appId;

                if (appId != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        AppId = appId,
                        LogText = $"{userName} 批量撤销编辑状态的配置"
                    };
                    Task.Run(async () =>
                    {
                        using (var syslogService = NewSysLogService())
                        {
                            await syslogService.AddSysLogAsync(log);
                        }
                    });
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
                Task.Run(async () => {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_NODE_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                ServerNode node = param_dy.node;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    LogText = $"用户：{userName} 删除节点：{node.Address}"
                };
                Task.Run(async () => {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
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
                Task.Run(async () => {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
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
                Task.Run(async () => {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_USER_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                User user = param_dy.user;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    LogText = $"用户：{userName} 删除用户：{user.UserName} 成功"
                };
                Task.Run(async () => {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
            });

            TinyEventBus.Instance.Regist(EventKeys.DISCONNECT_CLIENT_SUCCESS, (param) =>
            {
                dynamic param_dy = param;
                string clientId = param_dy.clientId;
                string userName = param_dy.userName;

                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    LogText = $"用户：{userName} 断开客户端 {clientId} 成功"
                };
                Task.Run(async () => {
                    using (var syslogService = NewSysLogService())
                    {
                        await syslogService.AddSysLogAsync(log);
                    }
                });
            });


        }

        /// <summary>
        /// 根据当前配置计算需要通知的应用
        /// </summary>
        /// <param name="currentUpdateConfig"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, WebsocketAction>> GetNeedNoticeInheritancedFromAppsAction(string appId)
        {
            Dictionary<string, WebsocketAction> needNoticeAppsActions = new Dictionary<string, WebsocketAction>
            {
            };
            using (var appService = GetAppService())
            {
                var currentApp = await appService.GetAsync(appId);
                if (currentApp.Type == AppType.Inheritance)
                {
                    var inheritancedFromApps = await appService.GetInheritancedFromAppsAsync(appId);
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

    }
}
