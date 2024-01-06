using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service.EventRegisterService;

internal class ConfigStatusUpdateRegister : IEventRegister
{
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private readonly IServerNodeService _serverNodeService;
    private readonly IAppService _appService;

    public ConfigStatusUpdateRegister(
        IRemoteServerNodeProxy remoteServerNodeProxy,
        EventRegisterTransient<IServerNodeService> serverNodeServiceAccessor,
        EventRegisterTransient<IAppService> appServiceAccessor)
    {
        _remoteServerNodeProxy = remoteServerNodeProxy;
        _serverNodeService = serverNodeServiceAccessor();
        _appService = appServiceAccessor();
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
                    {
                        var nodes = await _serverNodeService.GetAllNodesAsync();
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
                            await _remoteServerNodeProxy.ClearConfigServiceCache(node.Id);
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
                                    node.Id,
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
                    {
                        var nodes = await _serverNodeService.GetAllNodesAsync();
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
                                await _remoteServerNodeProxy.AppClientsDoActionAsync(node.Id, item.Key,
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
        {
            var currentApp = await _appService.GetAsync(appId);
            if (currentApp.Type == AppType.Inheritance)
            {
                var inheritancedFromApps = await _appService.GetInheritancedFromAppsAsync(appId);
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