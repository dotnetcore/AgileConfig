using AgileConfig.Server.EventHandler;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

public class ConfigStatusUpdateEventHandlersRegister : IEventHandlerRegister
{
    private readonly Common.EventBus.ITinyEventBus _tinyEventBus;

    public ConfigStatusUpdateEventHandlersRegister(Common.EventBus.ITinyEventBus tinyEventBus)
    {
        _tinyEventBus = tinyEventBus;
    }

    public void Register()
    {
        _tinyEventBus.Register<ConfigPublishedHandler>();
        _tinyEventBus.Register<ConfigStatusRollbackSuccessfulThenNoticeClientReloadHandler>();
    }
}