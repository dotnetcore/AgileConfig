using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.EventHandler;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

public class ServiceInfoStatusUpdateEventHandlersRegister : IEventHandlerRegister
{
    private readonly ITinyEventBus _tinyEventBus;

    public ServiceInfoStatusUpdateEventHandlersRegister(ITinyEventBus tinyEventBus)
    {
        _tinyEventBus = tinyEventBus;
    }

    public void Register()
    {
        _tinyEventBus.Register<ServiceRegisterHandler>();
        _tinyEventBus.Register<ServiceUnRegisterHandler>();
        _tinyEventBus.Register<ServiceStatusUpdateHandler>();
    }
}