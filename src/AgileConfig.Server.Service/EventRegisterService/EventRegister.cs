#nullable enable
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

public class EventRegister(EventRegisterResolver eventRegisterResolver) : IEventRegister
{
    private readonly IEventRegister? _configStatusUpdateRegister = eventRegisterResolver(nameof(ConfigStatusUpdateRegister));
    private readonly IEventRegister? _sysLogRegister = eventRegisterResolver(nameof(SysLogRegister));
    private readonly IEventRegister? _serviceInfoStatusUpdateRegister = eventRegisterResolver(nameof(ServiceInfoStatusUpdateRegister));

    public void Register()
    {
        _configStatusUpdateRegister?.Register();
        _sysLogRegister?.Register();
        _serviceInfoStatusUpdateRegister?.Register();
    }
}