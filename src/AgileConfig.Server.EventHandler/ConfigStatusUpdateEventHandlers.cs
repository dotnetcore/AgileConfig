using Agile.Config.Protocol;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.EventHandler
{
    public class ConfigPublishedHandler : IEventHandler<PublishConfigSuccessful>
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IServerNodeService _serverNodeService;
        private readonly IAppService _appService;

        public ConfigPublishedHandler(
            IRemoteServerNodeProxy remoteServerNodeProxy,
            IServerNodeService serverNodeService,
            IAppService appService)
        {
            _remoteServerNodeProxy = remoteServerNodeProxy;
            _serverNodeService = serverNodeService;
            _appService = appService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as PublishConfigSuccessful;
            var timelineNode = evtInstance?.PublishTimeline;
            if (timelineNode != null)
            {
                var nodes = await _serverNodeService.GetAllNodesAsync();
                var noticeApps = await ConfigUpadteNoticeUtil.GetNeedNoticeInheritancedFromAppsAction(_appService, timelineNode.AppId);
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
        }

    }

    public class ConfigStatusRollbackSuccessfulThenNoticeClientReloadHandler : IEventHandler<RollbackConfigSuccessful>
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IServerNodeService _serverNodeService;
        private readonly IAppService _appService;

        public ConfigStatusRollbackSuccessfulThenNoticeClientReloadHandler(
            IRemoteServerNodeProxy remoteServerNodeProxy,
            IServerNodeService serverNodeService,
            IAppService appService)
        {
            _remoteServerNodeProxy = remoteServerNodeProxy;
            _serverNodeService = serverNodeService;
            _appService = appService;
        }

        public async Task Handle(IEvent evt)
        {
            var evtInstance = evt as RollbackConfigSuccessful;
            var appId = evtInstance.TimelineNode.AppId;
            var env = evtInstance.TimelineNode.Env;
            var nodes = await _serverNodeService.GetAllNodesAsync();
            var noticeApps = await ConfigUpadteNoticeUtil.GetNeedNoticeInheritancedFromAppsAction(_appService, appId);
            noticeApps.Add(appId,
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
                        env,
                        item.Value);
                }
            }
        }


    }

    class ConfigUpadteNoticeUtil
    {
        /// <summary>
        /// Determine which applications need to be notified based on the current configuration.
        /// </summary>
        /// <returns></returns>
        public static async Task<Dictionary<string, WebsocketAction>> GetNeedNoticeInheritancedFromAppsAction(IAppService appService, string appId)
        {
            Dictionary<string, WebsocketAction> needNoticeAppsActions = new Dictionary<string, WebsocketAction>
            {
            };
            var currentApp = await appService.GetAsync(appId);
            if (currentApp.Type == AppType.Inheritance)
            {
                var inheritancedFromApps = await appService.GetInheritancedFromAppsAsync(appId);
                inheritancedFromApps.ForEach(x =>
                {
                    needNoticeAppsActions.Add(x.Id, new WebsocketAction
                    {
                        Action = ActionConst.Reload,
                        Module = ActionModule.ConfigCenter
                    });
                });
            }

            return needNoticeAppsActions;
        }
    }
}
