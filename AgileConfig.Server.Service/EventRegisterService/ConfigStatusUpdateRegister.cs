using System.Collections.Generic;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

internal class ConfigStatusUpdateRegister : IEventRegister
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;

    public ConfigStatusUpdateRegister(IRemoteServerNodeProxy remoteServerNodeProxy)
    {
        _remoteServerNodeProxy = remoteServerNodeProxy;
    }

    private IServerNodeService NewServerNodeService()
    {
        return new ServerNodeService(new FreeSqlContext(FreeSQL.Instance));
    }

    private IAppService NewAppService()
    {
        return new AppService(new FreeSqlContext(FreeSQL.Instance));
    }

    private IConfigService NewConfigService()
    {
        return new ConfigService(NewAppService(), new SettingService(new FreeSqlContext(FreeSQL.Instance)),
            new UserService(new FreeSqlContext(FreeSQL.Instance)));
    }

    public void Register()
    {
        TinyEventBus.Instance.Register(EventKeys.PUBLISH_CONFIG_SUCCESS, (param) =>
        {
            dynamic param_dy = param;
            PublishTimeline timelineNode = param_dy.publishTimelineNode;
            if (timelineNode != null)
            {
                Task.Run(async () =>
                {
                    using (var serverNodeService = NewServerNodeService())
                    {
                        var nodes = await serverNodeService.GetAllNodesAsync();
                        var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(timelineNode.AppId);
                        noticeApps.Add(timelineNode.AppId,
                            new WebsocketAction { Action = ActionConst.Reload, Module = ActionModule.ConfigCenter });

                        foreach (var node in nodes)
                        {
                            if (node.Status == NodeStatus.Offline)
                            {
                                continue;
                            }

                            //all server cache
                            await _remoteServerNodeProxy.ClearConfigServiceCache(node.Address);
                        }

                        foreach (var node in nodes)
                        {
                            if (node.Status == NodeStatus.Offline)
                            {
                                continue;
                            }

                            foreach (var item in noticeApps)
                            {
                                await _remoteServerNodeProxy.AppClientsDoActionAsync(
                                    node.Address,
                                    item.Key,
                                    timelineNode.Env,
                                    item.Value);
                            }
                        }
                    }
                });
            }
        });

        TinyEventBus.Instance.Register(EventKeys.ROLLBACK_CONFIG_SUCCESS, (param) =>
        {
            dynamic param_dy = param;
            PublishTimeline timelineNode = param_dy.timelineNode;
            if (timelineNode != null)
            {
                Task.Run(async () =>
                {
                    using (var serverNodeService = NewServerNodeService())
                    {
                        var nodes = await serverNodeService.GetAllNodesAsync();
                        var noticeApps = await GetNeedNoticeInheritancedFromAppsAction(timelineNode.AppId);
                        noticeApps.Add(timelineNode.AppId,
                            new WebsocketAction
                                { Action = ActionConst.Reload, Module = ActionModule.ConfigCenter });

                        foreach (var node in nodes)
                        {
                            if (node.Status == NodeStatus.Offline)
                            {
                                continue;
                            }

                            foreach (var item in noticeApps)
                            {
                                await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Address, item.Key,
                                    timelineNode.Env,
                                    item.Value);
                            }
                        }
                    }
                });
            }
        });
    }

    /// <summary>
    /// 根据当前配置计算需要通知的应用
    /// </summary>
    /// <returns></returns>
    private async Task<Dictionary<string, WebsocketAction>> GetNeedNoticeInheritancedFromAppsAction(string appId)
    {
        Dictionary<string, WebsocketAction> needNoticeAppsActions = new Dictionary<string, WebsocketAction>
        {
        };
        using (var appService = NewAppService())
        {
            var currentApp = await appService.GetAsync(appId);
            if (currentApp.Type == AppType.Inheritance)
            {
                var inheritancedFromApps = await appService.GetInheritancedFromAppsAsync(appId);
                inheritancedFromApps.ForEach(x =>
                {
                    needNoticeAppsActions.Add(x.Id, new WebsocketAction
                    {
                        Action = ActionConst.Reload, Module = ActionModule.ConfigCenter
                    });
                });
            }

            return needNoticeAppsActions;
        }
    }
}