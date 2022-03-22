using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

internal class SysLogRegister : IEventRegister
{
    private ISysLogService NewSysLogService()
    {
        return new SysLogService(new FreeSqlContext(FreeSQL.Instance));
    }

    public void Register()
    {
        TinyEventBus.Instance.Register(EventKeys.USER_LOGIN_SUCCESS, (param) =>
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

        TinyEventBus.Instance.Register(EventKeys.INIT_SUPERADMIN_PASSWORD_SUCCESS, (parm) =>
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

        TinyEventBus.Instance.Register(EventKeys.RESET_USER_PASSWORD_SUCCESS, (param) =>
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

        TinyEventBus.Instance.Register(EventKeys.CHANGE_USER_PASSWORD_SUCCESS, (param) =>
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

        TinyEventBus.Instance.Register(EventKeys.ADD_APP_SUCCESS, (param) =>
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
        TinyEventBus.Instance.Register(EventKeys.EDIT_APP_SUCCESS, (param) =>
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

        TinyEventBus.Instance.Register(EventKeys.DISABLE_OR_ENABLE_APP_SUCCESS, (param) =>
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

        TinyEventBus.Instance.Register(EventKeys.DELETE_APP_SUCCESS, (param) =>
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
        TinyEventBus.Instance.Register(EventKeys.ADD_CONFIG_SUCCESS, (param) =>
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
                    LogText =
                        $"用户：{userName} 新增配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】【Env：{config.Env}】【待发布】"
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
        TinyEventBus.Instance.Register(EventKeys.EDIT_CONFIG_SUCCESS, (param) =>
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
                    LogText =
                        $"用户：{userName} 编辑配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】【Env：{config.Env}】【待发布】"
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

        TinyEventBus.Instance.Register(EventKeys.DELETE_CONFIG_SUCCESS, (param) =>
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
                    LogText =
                        $"用户：{userName} 删除配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】【Env：{config.Env}】【待发布】"
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
        TinyEventBus.Instance.Register(EventKeys.DELETE_CONFIG_SOME_SUCCESS, (param) =>
        {
            dynamic param_dy = param;
            string userName = param_dy.userName;
            string appId = param_dy.appId;
            string env = param_dy.env;
            if (appId != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    AppId = appId,
                    LogText = $"用户：{userName} 批量删除配置【Env：{env}】"
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

        TinyEventBus.Instance.Register(EventKeys.PUBLISH_CONFIG_SUCCESS, (param) =>
        {
            dynamic param_dy = param;
            PublishTimeline node = param_dy.publishTimelineNode;
            string userName = param_dy.userName;
            string env = param_dy.env;
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                AppId = node.AppId,
                LogText =
                    $"用户：{userName} 发布配置【AppId：{node.AppId}】【Env：{env}】【版本：{node.PublishTime.Value:yyyyMMddHHmmss}】"
            };
            Task.Run(async () =>
            {
                using (var syslogService = NewSysLogService())
                {
                    await syslogService.AddSysLogAsync(log);
                }
            });
        });
        TinyEventBus.Instance.Register(EventKeys.ROLLBACK_CONFIG_SUCCESS, (param) =>
        {
            dynamic param_dy = param;
            string userName = param_dy.userName;
            PublishTimeline timelineNode = param_dy.timelineNode;
            string env = param_dy.env;

            if (timelineNode != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    AppId = timelineNode.AppId,
                    LogText =
                        $"{userName} 回滚应用【{timelineNode.AppId}】【Env：{env}】至发布版本【{timelineNode.PublishTime.Value:yyyyMMddHHmmss}】"
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
        TinyEventBus.Instance.Register(EventKeys.CANCEL_EDIT_CONFIG_SUCCESS, (param) =>
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
                    LogText =
                        $"{userName} 撤销编辑状态的配置【Group：{config.Group}】【Key：{config.Key}】【AppId：{config.AppId}】【Env：{config.Env}】"
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
        TinyEventBus.Instance.Register(EventKeys.CANCEL_EDIT_CONFIG_SOME_SUCCESS, (param) =>
        {
            dynamic param_dy = param;
            string userName = param_dy.userName;
            string appId = param_dy.appId;
            string env = param_dy.env;

            if (appId != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = appId,
                    LogText = $"{userName} 批量撤销编辑状态的配置【Env：{env}】"
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
        TinyEventBus.Instance.Register(EventKeys.ADD_NODE_SUCCESS, (param) =>
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
            Task.Run(async () =>
            {
                using (var syslogService = NewSysLogService())
                {
                    await syslogService.AddSysLogAsync(log);
                }
            });
        });

        TinyEventBus.Instance.Register(EventKeys.DELETE_NODE_SUCCESS, (param) =>
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
            Task.Run(async () =>
            {
                using (var syslogService = NewSysLogService())
                {
                    await syslogService.AddSysLogAsync(log);
                }
            });
        });

        TinyEventBus.Instance.Register(EventKeys.ADD_USER_SUCCESS, (param) =>
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
            Task.Run(async () =>
            {
                using (var syslogService = NewSysLogService())
                {
                    await syslogService.AddSysLogAsync(log);
                }
            });
        });

        TinyEventBus.Instance.Register(EventKeys.EDIT_USER_SUCCESS, (param) =>
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
            Task.Run(async () =>
            {
                using (var syslogService = NewSysLogService())
                {
                    await syslogService.AddSysLogAsync(log);
                }
            });
        });

        TinyEventBus.Instance.Register(EventKeys.DELETE_USER_SUCCESS, (param) =>
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
            Task.Run(async () =>
            {
                using (var syslogService = NewSysLogService())
                {
                    await syslogService.AddSysLogAsync(log);
                }
            });
        });

        TinyEventBus.Instance.Register(EventKeys.DISCONNECT_CLIENT_SUCCESS, (param) =>
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
            Task.Run(async () =>
            {
                using (var syslogService = NewSysLogService())
                {
                    await syslogService.AddSysLogAsync(log);
                }
            });
        });

        //service info envets
        TinyEventBus.Instance.Register(EventKeys.REGISTER_A_SERVICE, (param) =>
        {
            dynamic param_dy = param;
            string serviceId = param_dy.ServiceId;
            string serviceName = param_dy.ServiceName;
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"服务：【{serviceId}】【{serviceName}】 注册成功"
            };
            Task.Run(async () =>
            {
                using var syslogService = NewSysLogService();
                await syslogService.AddSysLogAsync(log);
            });
        });
        TinyEventBus.Instance.Register(EventKeys.UNREGISTER_A_SERVICE, (param) =>
        {
            dynamic param_dy = param;
            string serviceId = param_dy.ServiceId;
            string serviceName = param_dy.ServiceName;
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"服务：【{serviceId}】【{serviceName}】 卸载成功"
            };
            Task.Run(async () =>
            {
                using var syslogService = NewSysLogService();
                await syslogService.AddSysLogAsync(log);
            });
        });
    }
}