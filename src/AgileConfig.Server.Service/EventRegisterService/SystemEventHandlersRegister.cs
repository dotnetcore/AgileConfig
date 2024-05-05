using AgileConfig.Server.EventHandler;
using AgileConfig.Server.IService;
using ITinyEventBus = AgileConfig.Server.Common.EventBus.ITinyEventBus;

namespace AgileConfig.Server.Service.EventRegisterService;

public class SystemEventHandlersRegister : IEventHandlerRegister
{
    private readonly ITinyEventBus _tinyEventBus;

    public SystemEventHandlersRegister(ITinyEventBus tinyEventBus)
    {
        _tinyEventBus = tinyEventBus;
    }

    public void Register()
    {
        _tinyEventBus.Register<LoginEventHandler>();
        _tinyEventBus.Register<InitSaPasswordEventHandler>();
        _tinyEventBus.Register<ResetUserPasswordEventHandler>();
        _tinyEventBus.Register<ChangeUserPasswordEventHandler>();
        _tinyEventBus.Register<AddAppEventHandler>();
        _tinyEventBus.Register<EditAppEventHandler>();
        _tinyEventBus.Register<DisableOrEnableAppEventHandler>();
        _tinyEventBus.Register<DeleteAppEventHandler>();
        _tinyEventBus.Register<AddConfigEventHandler>();
        _tinyEventBus.Register<EditConfigEventHandler>();
        _tinyEventBus.Register<DeleteConfigEventHandler>();
        _tinyEventBus.Register<DeleteSomeConfigEventHandler>();
        _tinyEventBus.Register<PublishConfigEventHandler>();
        _tinyEventBus.Register<RollbackConfigEventHandler>();
        _tinyEventBus.Register<DisContectClientEventHandler>();
        _tinyEventBus.Register<RegisterAServiceEventHandler>();
        _tinyEventBus.Register<UnRegisterAServiceEventHandler>();
        _tinyEventBus.Register<AddNodeEventHandler>();
        _tinyEventBus.Register<DeleteNodeEventHandler>();
        _tinyEventBus.Register<AddUserEventHandler>();
        _tinyEventBus.Register<EditUserEventHandler>();
        _tinyEventBus.Register<DeleteUserEventHandler>();

    }
}