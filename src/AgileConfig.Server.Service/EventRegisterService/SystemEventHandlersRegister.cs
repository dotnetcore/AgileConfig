using AgileConfig.Server.EventHandler;
using AgileConfig.Server.IService;
using ITinyEventBus = AgileConfig.Server.Common.EventBus.ITinyEventBus;

namespace AgileConfig.Server.Service.EventRegisterService;

public class SystemEventHandlersRegister(ITinyEventBus tinyEventBus) : IEventHandlerRegister
{
    public void Register()
    {
        tinyEventBus.Register<LoginEventHandler>();
        tinyEventBus.Register<InitSaPasswordEventHandler>();
        tinyEventBus.Register<ResetUserPasswordEventHandler>();
        tinyEventBus.Register<ChangeUserPasswordEventHandler>();
        tinyEventBus.Register<AddAppEventHandler>();
        tinyEventBus.Register<EditAppEventHandler>();
        tinyEventBus.Register<DisableOrEnableAppEventHandler>();
        tinyEventBus.Register<DeleteAppEventHandler>();
        tinyEventBus.Register<AddConfigEventHandler>();
        tinyEventBus.Register<EditConfigEventHandler>();
        tinyEventBus.Register<DeleteConfigEventHandler>();
        tinyEventBus.Register<DeleteSomeConfigEventHandler>();
        tinyEventBus.Register<PublishConfigEventHandler>();
        tinyEventBus.Register<RollbackConfigEventHandler>();
        tinyEventBus.Register<DisContectClientEventHandler>();
        tinyEventBus.Register<RegisterAServiceEventHandler>();
        tinyEventBus.Register<UnRegisterAServiceEventHandler>();
        tinyEventBus.Register<AddNodeEventHandler>();
        tinyEventBus.Register<DeleteNodeEventHandler>();
        tinyEventBus.Register<AddUserEventHandler>();
        tinyEventBus.Register<EditUserEventHandler>();
        tinyEventBus.Register<DeleteUserEventHandler>();

    }
}