using AgileConfig.Server.IService;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service.EventRegisterService;

public class EventRegister : IEventRegister
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private readonly ConfigStatusUpdateRegister _configStatusUpdateRegister;
    private readonly SysLogRegister _sysLogRegister;
    private readonly ServiceInfoStatusUpdateRegister _serviceInfoStatusUpdateRegister;

    public EventRegister(IRemoteServerNodeProxy remoteServerNodeProxy, ILoggerFactory loggerFactory)
    {
        _remoteServerNodeProxy = remoteServerNodeProxy;
        _configStatusUpdateRegister = new ConfigStatusUpdateRegister(_remoteServerNodeProxy);
        _sysLogRegister = new SysLogRegister();
        _serviceInfoStatusUpdateRegister = new ServiceInfoStatusUpdateRegister(_remoteServerNodeProxy, loggerFactory);
    }

    public void Register()
    {
        _configStatusUpdateRegister.Register();
        _sysLogRegister.Register();
        _serviceInfoStatusUpdateRegister.Register();
    }
}