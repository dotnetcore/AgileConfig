#nullable enable
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

public class EventHandlerRegister(SystemEventHandlersRegister sysLogRegister, 
    ConfigStatusUpdateEventHandlersRegister configStatusUpdateRegister,
    ServiceInfoStatusUpdateEventHandlersRegister serviceInfoStatusUpdateRegister
    ) : IEventHandlerRegister
{
    private readonly IEventHandlerRegister? _sysLogRegister = sysLogRegister;
    private readonly IEventHandlerRegister? _configStatusUpdateRegister = configStatusUpdateRegister;
    private readonly IEventHandlerRegister? _serviceInfoStatusUpdateRegister = serviceInfoStatusUpdateRegister;

    public void Register()
    {
        _configStatusUpdateRegister?.Register();
        _sysLogRegister?.Register();
        _serviceInfoStatusUpdateRegister?.Register();
    }
}