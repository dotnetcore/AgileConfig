using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using AgileHttp;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service.EventRegisterService;

internal class ServiceInfoStatusUpdateRegister : IEventRegister
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private ILogger _logger;

    public ServiceInfoStatusUpdateRegister(IRemoteServerNodeProxy remoteServerNodeProxy, ILoggerFactory loggerFactory)
    {
        _remoteServerNodeProxy = remoteServerNodeProxy;
        _logger = loggerFactory.CreateLogger<ServiceInfoStatusUpdateRegister>();
    }

    private IServerNodeService NewServerNodeService()
    {
        return new ServerNodeService(new FreeSqlContext(FreeSQL.Instance));
    }

    private IServiceInfoService NewServiceInfoService()
    {
        return new ServiceInfoService(null);
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
                    //clear cache
                    _ = _remoteServerNodeProxy.ClearServiceInfoCache(serverNode.Address);
                    //send ws action
                    var act = new WebsocketAction()
                    {
                        Module = ActionModule.RegisterCenter,
                        Action = ActionConst.Reload
                    };
                    _ = _remoteServerNodeProxy.AllClientsDoActionAsync(serverNode.Address, act);
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
                    //clear cache
                    _ = _remoteServerNodeProxy.ClearServiceInfoCache(serverNode.Address);
                    //send ws action
                    var act = new WebsocketAction()
                    {
                        Module = ActionModule.RegisterCenter,
                        Action = ActionConst.Reload
                    };
                    _ = _remoteServerNodeProxy.AllClientsDoActionAsync(serverNode.Address, act);
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
                    //clear cache
                    _ = _remoteServerNodeProxy.ClearServiceInfoCache(serverNode.Address);
                    //send ws action
                    var act = new WebsocketAction()
                    {
                        Module = ActionModule.RegisterCenter,
                        Action = ActionConst.Reload
                    };
                    _ = _remoteServerNodeProxy.AllClientsDoActionAsync(serverNode.Address, act);
                }
            });
        });
        TinyEventBus.Instance.Register(EventKeys.UPDATE_SERVICE_STATUS, (param) =>
        {
            Task.Run(async () =>
            {
                dynamic paramObj = param;
                string id = paramObj.UniqueId;
                using var serviceInfoService = NewServiceInfoService();
                var service = await serviceInfoService.GetByUniqueIdAsync(id);
                if (service != null && !string.IsNullOrWhiteSpace(service.AlarmUrl) &&
                    service.Status == ServiceStatus.Unhealthy)
                {
                    //如果是下线发送通知
                    _ = SendServiceOfflineMessageAsync(service);
                }
            });
        });
    }

    private async Task SendServiceOfflineMessageAsync(ServiceInfo service)
    {
        var msg = new
        {
            UniqueId = service.Id,
            service.ServiceId,
            service.ServiceName,
            Time = DateTime.Now,
            Status = ServiceStatus.Unhealthy.ToString(),
            Message = "服务不健康"
        };

        try
        {
            await FunctionUtil.TRYAsync(async () =>
            {
                var resp = await service.AlarmUrl.AsHttp("POST", msg).Config(new RequestOptions()
                {
                    ContentType = "application/json"
                }).SendAsync();
                if (resp.Exception != null)
                {
                    throw resp.Exception;
                }

                return resp.StatusCode == HttpStatusCode.OK;
            }, 5);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"try to send message to alarm url {service.AlarmUrl} failed");
        }
    }
}