using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

public class EventRegister : IEventRegister
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private readonly ConfigWebsoketActionRegister _configWebsoketActionRegister;
    private readonly SysLogRegister _sysLogRegister;

    public EventRegister(IRemoteServerNodeProxy remoteServerNodeProxy)
    {
        _remoteServerNodeProxy = remoteServerNodeProxy;
        _configWebsoketActionRegister = new ConfigWebsoketActionRegister(_remoteServerNodeProxy);
        _sysLogRegister = new SysLogRegister();
    }

    public void Register()
    {
        _configWebsoketActionRegister.Register();
        _sysLogRegister.Register();
    }
}