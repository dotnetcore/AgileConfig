using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.EventHandler
{
    public class LoginEventHandler : IEventHandler<LoginEvent>
    {
        private readonly ISysLogService _sysLogService;

        public LoginEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            string userName = (evt as LoginEvent).UserName;
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"{userName} 登录成功"
            };
            await _sysLogService.AddSysLogAsync(log);
        }
    }

    public class InitSaPasswordEventHandler : IEventHandler<InitSaPasswordSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public InitSaPasswordEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"超级管理员密码初始化成功"
            };
            await _sysLogService.AddSysLogAsync(log);
        }
    }

    public class ResetUserPasswordEventHandler : IEventHandler<ResetUserPasswordSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public ResetUserPasswordEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as ResetUserPasswordSuccessful;
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"用户 {evtInstance.OpUser} 重置 {evtInstance.UserName} 的密码为默认密码 "
            };
            await _sysLogService.AddSysLogAsync(log);
        }
    }

    public class ChangeUserPasswordEventHandler : IEventHandler<ChangeUserPasswordSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public ChangeUserPasswordEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as ChangeUserPasswordSuccessful;
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"修改用户 {evtInstance.UserName} 的密码成功"
            };
            await _sysLogService.AddSysLogAsync(log);
        }
    }

    public class AddAppEventHandler : IEventHandler<AddAppSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public AddAppEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as AddAppSuccessful;
            App app = evtInstance.App;
            string userName = evtInstance.UserName;
            if (app != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 新增应用【AppId：{app.Id}】【AppName：{app.Name}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class EditAppEventHandler : IEventHandler<EditAppSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public EditAppEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as EditAppSuccessful;
            App app = evtInstance.App;
            string userName = evtInstance.UserName;
            if (app != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 编辑应用【AppId：{app.Id}】【AppName：{app.Name}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class DisableOrEnableAppEventHandler : IEventHandler<DisableOrEnableAppSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public DisableOrEnableAppEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as DisableOrEnableAppSuccessful;
            App app = evtInstance.App;
            string userName = evtInstance.UserName;
            if (app != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} {(app.Enabled ? "启用" : "禁用")}应用【AppId:{app.Id}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class DeleteAppEventHandler : IEventHandler<DeleteAppSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public DeleteAppEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as DeleteAppSuccessful;
            App app = evtInstance.App;
            string userName = evtInstance.UserName;
            if (app != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    LogText = $"用户：{userName} 删除应用【AppId:{app.Id}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class AddConfigEventHandler : IEventHandler<AddConfigSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public AddConfigEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as AddConfigSuccessful;
            Config config = evtInstance.Config;
            string userName = evtInstance.UserName;
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
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class EditConfigEventHandler : IEventHandler<EditConfigSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public EditConfigEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as EditConfigSuccessful;
            Config config = evtInstance.Config;
            string userName = evtInstance.UserName;
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
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class DeleteConfigEventHandler : IEventHandler<DeleteConfigSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public DeleteConfigEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as DeleteConfigSuccessful;
            Config config = evtInstance.Config;
            string userName = evtInstance.UserName;
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
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class DeleteSomeConfigEventHandler : IEventHandler<DeleteSomeConfigSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public DeleteSomeConfigEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as DeleteSomeConfigSuccessful;
            Config config = evtInstance.Config;
            string userName = evtInstance.UserName;
            string env = evtInstance.Config.Env;
            if (config != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    AppId = config.AppId,
                    LogText = $"用户：{userName} 批量删除配置【Env：{env}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class PublishConfigEventHandler : IEventHandler<PublishConfigSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public PublishConfigEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as PublishConfigSuccessful;
            var node = evtInstance.PublishTimeline;
            string userName = evtInstance.UserName;
            string env = node.Env;
            if (node != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = node.AppId,
                    LogText =
                    $"用户：{userName} 发布配置【AppId：{node.AppId}】【Env：{env}】【版本：{node.PublishTime.Value:yyyyMMddHHmmss}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class RollbackConfigEventHandler : IEventHandler<RollbackConfigSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public RollbackConfigEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as RollbackConfigSuccessful;
            var node = evtInstance.TimelineNode;
            string userName = evtInstance.UserName;
            string env = node.Env;
            if (node != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    AppId = node.AppId,
                    LogText =
                        $"{userName} 回滚应用【{node.AppId}】【Env：{env}】至发布版本【{node.PublishTime.Value:yyyyMMddHHmmss}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class CancelEditConfigEventHandler : IEventHandler<CancelEditConfigSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public CancelEditConfigEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as CancelEditConfigSuccessful;
            var config = evtInstance.Config;
            string userName = evtInstance.UserName;
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
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class CancelEditConfigSomeConfig : IEventHandler<CancelEditConfigSomeSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public CancelEditConfigSomeConfig(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as CancelEditConfigSomeSuccessful;
            string userName = evtInstance.UserName;
            var config = evtInstance.Config;
            var env = config.Env;

            if (config != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    AppId = config.AppId,
                    LogText = $"{userName} 批量撤销编辑状态的配置【Env：{env}】"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class AddNodeEventHandler : IEventHandler<AddNodeSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public AddNodeEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as AddNodeSuccessful;
            string userName = evtInstance.UserName;
            var node = evtInstance.Node;

            if (node != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 添加节点：{node.Id}"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class DeleteNodeEventHandler : IEventHandler<DeleteNodeSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public DeleteNodeEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as DeleteNodeSuccessful;
            string userName = evtInstance.UserName;
            var node = evtInstance.Node;

            if (node != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    LogText = $"用户：{userName} 删除节点：{node.Id}"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class AddUserEventHandler : IEventHandler<AddUserSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public AddUserEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as AddUserSuccessful;
            string userName = evtInstance.UserName;
            var user = evtInstance.User;

            if (user != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 添加用户：{user.UserName} 成功"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class EditUserEventHandler : IEventHandler<EditUserSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public EditUserEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as EditUserSuccessful;
            string userName = evtInstance.UserName;
            var user = evtInstance.User;

            if (user != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"用户：{userName} 编辑用户：{user.UserName} 成功"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class DeleteUserEventHandler : IEventHandler<DeleteUserSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public DeleteUserEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as DeleteUserSuccessful;
            string userName = evtInstance.UserName;
            var user = evtInstance.User;

            if (user != null)
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Warn,
                    LogText = $"用户：{userName} 删除用户：{user.UserName} 成功"
                };
                await _sysLogService.AddSysLogAsync(log);
            }
        }
    }

    public class DisContectClientEventHandler : IEventHandler<DiscoinnectSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public DisContectClientEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
            };
            await _sysLogService.AddSysLogAsync(log);
        }

    }

    public class RegisterAServiceEventHandler : IEventHandler<RegisterAServiceSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public RegisterAServiceEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as RegisterAServiceSuccessful;

            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"服务：【{evtInstance.ServiceId}】【{evtInstance.ServiceName}】 注册成功"
            };
            await _sysLogService.AddSysLogAsync(log);

        }
    }

    public class UnRegisterAServiceEventHandler : IEventHandler<UnRegisterAServiceSuccessful>
    {
        private readonly ISysLogService _sysLogService;

        public UnRegisterAServiceEventHandler(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as UnRegisterAServiceSuccessful;

            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"服务：【{evtInstance.ServiceId}】【{evtInstance.ServiceName}】 卸载成功"
            };
            await _sysLogService.AddSysLogAsync(log);

        }
    }
}
