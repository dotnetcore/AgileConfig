using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.EventHandler;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

public class ServiceInfoStatusUpdateEventHandlersRegister : IEventHandlerRegister
{

    private readonly Common.EventBus.ITinyEventBus _tinyEventBus;

    public ServiceInfoStatusUpdateEventHandlersRegister(Common.EventBus.ITinyEventBus tinyEventBus)
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