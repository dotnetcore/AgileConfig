using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

internal class ServiceInfoStatusUpdateRegister : IEventRegister
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;

    public ServiceInfoStatusUpdateRegister(IRemoteServerNodeProxy remoteServerNodeProxy)
    {
        _remoteServerNodeProxy = remoteServerNodeProxy;
    }

    private IServerNodeService NewServerNodeService()
    {
        return new ServerNodeService(new FreeSqlContext(FreeSQL.Instance));
    }

    public void Register()
    {
        TinyEventBus.Instance.Register(EventKeys.REGISTER_A_SERVICE, (param) =>
        {
            Task.Run(async () =>
            {
                using var serverNodeService = NewServerNodeService();
                var serverNodes = await serverNodeService.GetAllNodesAsync();
                foreach (var serverNode in serverNodes.Where(x => x.Status == NodeStatus.Online))
                {
                    _ = _remoteServerNodeProxy.ClearServiceInfoCache(serverNode.Address);
                }
            });
        });
        TinyEventBus.Instance.Register(EventKeys.UNREGISTER_A_SERVICE, (param) =>
        {
            Task.Run(async () =>
            {
                using var serverNodeService = NewServerNodeService();
                var serverNodes = await serverNodeService.GetAllNodesAsync();
                foreach (var serverNode in serverNodes.Where(x => x.Status == NodeStatus.Online))
                {
                    _ = _remoteServerNodeProxy.ClearServiceInfoCache(serverNode.Address);
                }
            });
        });
        TinyEventBus.Instance.Register(EventKeys.UPDATE_SERVICE_STATUS, (param) =>
        {
            Task.Run(async () =>
            {
                using var serverNodeService = NewServerNodeService();
                var serverNodes = await serverNodeService.GetAllNodesAsync();
                foreach (var serverNode in serverNodes.Where(x => x.Status == NodeStatus.Online))
                {
                    _ = _remoteServerNodeProxy.ClearServiceInfoCache(serverNode.Address);
                }
            });
        });
    }
}