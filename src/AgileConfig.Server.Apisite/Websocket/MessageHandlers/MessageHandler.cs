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
///     Message handler.
/// </summary>
internal class MessageHandler : IMessageHandler
{
    private readonly IConfigService _configService;
    private readonly IRegisterCenterService _registerCenterService;
    private readonly IServiceInfoService _serviceInfoService;

    public MessageHandler(
        IConfigService configService,
        IRegisterCenterService registerCenterService,
        IServiceInfoService serviceInfoService)
    {
        _configService = configService;
        _registerCenterService = registerCenterService;
        _serviceInfoService = serviceInfoService;
    }

    private int ClientVersion { get; set; }

    public bool Hit(HttpRequest request)
    {
        var ver = request.Headers["client-v"];
        if (string.IsNullOrEmpty(ver)) return false;

        if (int.TryParse(ver.ToString().Replace(".", ""), out var verInt))
        {
            ClientVersion = verInt;

            return verInt >= 160;
        }

        return false;
    }

    public async Task Handle(string message, HttpRequest request, WebsocketClient client)
    {
        message ??= "";

        // "ping" is old version
        if (message is "ping" or "c:ping")
        {
            // Reply with the MD5 of the local data for legacy heartbeat messages.
            var appId = request.Headers["appid"];
            appId = HttpUtility.UrlDecode(appId);
            var env = request.Headers["env"].ToString();
            ISettingService.IfEnvEmptySetDefault(ref env);

            var data = await GetCPingData(appId, env);

            await SendMessage(client.Client, JsonConvert.SerializeObject(new WebsocketAction
            {
                Action = ActionConst.Ping,
                Module = ActionModule.ConfigCenter,
                Data = data
            }));
        }
        else if (message.StartsWith("s:ping:"))
        {
            // Update the heartbeat timestamp for register-center clients.
            var id = message.Substring(7, message.Length - 7);
            var heartBeatResult = await _registerCenterService.ReceiveHeartbeatAsync(id);
            if (heartBeatResult)
            {
                var version = await _serviceInfoService.ServicesMD5Cache();
                await SendMessage(client.Client, JsonConvert.SerializeObject(new WebsocketAction
                {
                    Action = ActionConst.Ping,
                    Module = ActionModule.RegisterCenter,
                    Data = version
                }));
            }
        }
        else if (message == "loaded")
        {
            // The client reports that configuration data was loaded successfully.
            client.LastRefreshTime = DateTime.Now;
        }
        else
        {
            // Reply with 0 when the message cannot be handled.
            await SendMessage(client.Client, "0");
        }
    }

    private async Task SendMessage(WebSocket webSocket, string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }

    private async Task<string> GetCPingData(string appId, string env)
    {
        if (ClientVersion <= 176)
        {
            // For versions 1.7.6 and earlier, respond with V:md5.
            var md5 = await _configService.AppPublishedConfigsMd5CacheWithInheritance(appId, env);

            return md5;
        }

        // For versions 1.7.7 and later, respond with the publish timeline id.
        var publishTimeLineId = await _configService.GetLastPublishTimelineVirtualIdAsyncWithCache(appId, env);

        return publishTimeLineId;
    }
}