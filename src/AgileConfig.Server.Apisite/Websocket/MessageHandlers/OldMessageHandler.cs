using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Apisite.Websocket.MessageHandlers;



/// <summary>
/// Message handler used to remain compatible with legacy clients.
/// </summary>
internal class OldMessageHandler : IMessageHandler
{
    private readonly IConfigService _configService;
    public OldMessageHandler(IConfigService configService)
    {
        _configService = configService;
    }
    
    public bool Hit(HttpRequest request)
    {
        var ver = request.Headers["client-v"];
        return string.IsNullOrEmpty(ver);
    }
    private async Task SendMessage(WebSocket webSocket, string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
    public async Task Handle(string message, HttpRequest request, WebsocketClient client)
    {
        if (message == null)
        {
            message = "";
        }

        if (message == "ping")
        {
            // Support the legacy client heartbeat.
            // Reply with the MD5 of the local data when receiving "ping".
            var appId = request.Headers["appid"];
            var env = request.Headers["env"].ToString();
            env = ISettingService.IfEnvEmptySetDefault(ref env);
            var md5 = await _configService.AppPublishedConfigsMd5CacheWithInheritance(appId, env);
            await SendMessage(client.Client, $"V:{md5}");
        }
        else
        {
            // Reply with 0 when the message cannot be handled.
            await SendMessage(client.Client, "0");
        }
    }
}