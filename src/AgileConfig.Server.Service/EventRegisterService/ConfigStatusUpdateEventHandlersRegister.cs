using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.EventHandler;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

public class ConfigStatusUpdateEventHandlersRegister : IEventHandlerRegister
{
    private readonly ITinyEventBus _tinyEventBus;

    public ConfigStatusUpdateEventHandlersRegister(ITinyEventBus tinyEventBus)
    {
        _tinyEventBus = tinyEventBus;
    }

    public void Register()
    {
        _tinyEventBus.Register<ConfigPublishedHandler>();
        _tinyEventBus.Register<ConfigStatusRollbackSuccessfulThenNoticeClientReloadHandler>();
    }
}