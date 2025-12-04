using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.EventHandler;

public class LoginEventHandler : IEventHandler<LoginEvent>
{
    private readonly ISysLogService _sysLogService;

    public LoginEventHandler(ISysLogService sysLogService)
    {
        _sysLogService = sysLogService;
    }

    public async Task Handle(IEvent evt)
    {
        var userName = (evt as LoginEvent).UserName;
        var log = new SysLog
        {
            LogTime = DateTime.Now,
            LogType = SysLogType.Normal,
            LogText = $"{userName} login successful"
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
            LogText = "Super administrator password initialized successfully"
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
            LogText = $"User {evtInstance.OpUser} reset {evtInstance.UserName}'s password to default password"
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
            LogText = $"User {evtInstance.UserName} password changed successfully"
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
        var app = evtInstance.App;
        var userName = evtInstance.UserName;
        if (app != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"User: {userName} added app [AppId: {app.Id}] [AppName: {app.Name}]"
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
        var app = evtInstance.App;
        var userName = evtInstance.UserName;
        if (app != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"User: {userName} updated app [AppId: {app.Id}] [AppName: {app.Name}]"
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
        var app = evtInstance.App;
        var userName = evtInstance.UserName;
        if (app != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"User: {userName} {(app.Enabled ? "enabled" : "disabled")} app [AppId: {app.Id}]"
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
        var app = evtInstance.App;
        var userName = evtInstance.UserName;
        if (app != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Warn,
                LogText = $"User: {userName} deleted app [AppId: {app.Id}]"
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
        var config = evtInstance.Config;
        var userName = evtInstance.UserName;
        if (config != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                AppId = config.AppId,
                LogText =
                    $"User: {userName} added config [Group: {config.Group}] [Key: {config.Key}] [AppId: {config.AppId}] [Env: {config.Env}] [Pending publish]"
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
        var config = evtInstance.Config;
        var userName = evtInstance.UserName;
        if (config != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                AppId = config.AppId,
                LogText =
                    $"User: {userName} updated config [Group: {config.Group}] [Key: {config.Key}] [AppId: {config.AppId}] [Env: {config.Env}] [Pending publish]"
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
        var config = evtInstance.Config;
        var userName = evtInstance.UserName;
        if (config != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Warn,
                AppId = config.AppId,
                LogText =
                    $"User: {userName} deleted config [Group: {config.Group}] [Key: {config.Key}] [AppId: {config.AppId}] [Env: {config.Env}] [Pending publish]"
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
        var config = evtInstance.Config;
        var userName = evtInstance.UserName;
        var env = evtInstance.Config.Env;
        if (config != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Warn,
                AppId = config.AppId,
                LogText = $"User: {userName} batch deleted configs [Env: {env}]"
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
        var userName = evtInstance.UserName;
        var env = node.Env;
        if (node != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                AppId = node.AppId,
                LogText =
                    $"User: {userName} published config [AppId: {node.AppId}] [Env: {env}] [Version: {node.PublishTime.Value:yyyyMMddHHmmss}]"
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
        var userName = evtInstance.UserName;
        var env = node.Env;
        if (node != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Warn,
                AppId = node.AppId,
                LogText =
                    $"{userName} rolled back app [{node.AppId}] [Env: {env}] to published version [{node.PublishTime.Value:yyyyMMddHHmmss}]"
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
        var userName = evtInstance.UserName;
        if (config != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                AppId = config.AppId,
                LogText =
                    $"{userName} cancelled editing config [Group: {config.Group}] [Key: {config.Key}] [AppId: {config.AppId}] [Env: {config.Env}]"
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
        var userName = evtInstance.UserName;
        var config = evtInstance.Config;
        var env = config.Env;

        if (config != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                AppId = config.AppId,
                LogText = $"{userName} batch cancelled editing configs [Env: {env}]"
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
        var userName = evtInstance.UserName;
        var node = evtInstance.Node;

        if (node != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"User: {userName} added node: {node.Id}"
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
        var userName = evtInstance.UserName;
        var node = evtInstance.Node;

        if (node != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Warn,
                LogText = $"User: {userName} deleted node: {node.Id}"
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
        var userName = evtInstance.UserName;
        var user = evtInstance.User;

        if (user != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"User: {userName} added user: {user.UserName} successfully"
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
        var userName = evtInstance.UserName;
        var user = evtInstance.User;

        if (user != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Normal,
                LogText = $"User: {userName} updated user: {user.UserName} successfully"
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
        var userName = evtInstance.UserName;
        var user = evtInstance.User;

        if (user != null)
        {
            var log = new SysLog
            {
                LogTime = DateTime.Now,
                LogType = SysLogType.Warn,
                LogText = $"User: {userName} deleted user: {user.UserName} successfully"
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
            LogType = SysLogType.Normal
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
            LogText = $"Service: [{evtInstance.ServiceId}] [{evtInstance.ServiceName}] registered successfully"
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
            LogText = $"Service: [{evtInstance.ServiceId}] [{evtInstance.ServiceName}] unregistered successfully"
        };
        await _sysLogService.AddSysLogAsync(log);
    }
}