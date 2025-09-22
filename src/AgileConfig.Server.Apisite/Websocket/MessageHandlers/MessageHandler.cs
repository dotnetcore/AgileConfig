using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Agile.Config.Protocol;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Websocket.MessageHandlers;

/// <summary>
/// 消息处理器
/// </summary>
internal class MessageHandler : IMessageHandler
{
    private readonly IConfigService _configService;
    private readonly IRegisterCenterService _registerCenterService;
    private readonly IServiceInfoService _serviceInfoService;

    private int ClientVersion { get; set; }

    public MessageHandler(
        IConfigService configService,
        IRegisterCenterService registerCenterService,
        IServiceInfoService serviceInfoService)
    {
        _configService = configService;
        _registerCenterService = registerCenterService;
        _serviceInfoService = serviceInfoService;
    }

    public bool Hit(HttpRequest request)
    {
        var ver = request.Headers["client-v"];
        if (string.IsNullOrEmpty(ver))
        {
            return false;
        }

        if (int.TryParse(ver.ToString().Replace(".", ""), out var verInt))
        {
            ClientVersion = verInt;

            return verInt >= 160;
        }

        return false;
    }
    private async Task SendMessage(WebSocket webSocket, string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
    public async Task Handle(string message, HttpRequest request, WebsocketClient client)
    {
        message ??= "";

        // "ping" is old version
        if (message is "ping" or "c:ping")
        {
            //如果是ping，回复本地数据的md5版本 
            var appId = request.Headers["appid"];
            appId = HttpUtility.UrlDecode(appId);
            var env = request.Headers["env"].ToString();
            ISettingService.IfEnvEmptySetDefault(ref env);

            var data = await GetCPingData(appId, env);

            await SendMessage(client.Client, JsonConvert.SerializeObject(new WebsocketAction()
            {
                Action = ActionConst.Ping,
                Module = ActionModule.ConfigCenter,
                Data = data
            }));
        }
        else if (message.StartsWith("s:ping:"))
        {
            //如果是注册中心client的心跳，则更新client的心跳时间
            var id = message.Substring(7, message.Length - 7);
            var heartBeatResult = await _registerCenterService.ReceiveHeartbeatAsync(id);
            if (heartBeatResult)
            {
                var version = await _serviceInfoService.ServicesMD5Cache();
                await SendMessage(client.Client, JsonConvert.SerializeObject(new WebsocketAction()
                {
                    Action = ActionConst.Ping,
                    Module = ActionModule.RegisterCenter,
                    Data = version
                }));
            }
        }
        else if (message == "loaded")
        {
            //如果是client加载数据成功
            client.LastRefreshTime = DateTime.Now;
        }
        else
        {
            //如果无法处理，回复0
            await SendMessage(client.Client, "0");
        }
    }

    private async Task<string> GetCPingData(string appId, string env)
    {
        if (ClientVersion <= 176)
        {
            // 1.7.6及以前的版本，返回V:md5
            var md5 = await _configService.AppPublishedConfigsMd5CacheWithInheritance(appId, env);

            return md5;
        }
        else
        {
            // 1.7.7及以后的版本，返回 publish time line id
            var publishTimeLineId = await _configService.GetLastPublishTimelineVirtualIdAsyncWithCache(appId, env);

            return publishTimeLineId;
        }
    }
}