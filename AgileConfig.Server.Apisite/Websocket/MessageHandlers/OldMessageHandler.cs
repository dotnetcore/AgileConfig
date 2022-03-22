using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Apisite.Websocket.MessageHandlers;



/// <summary>
/// 消息处理器，用来兼容旧版本
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
    public async Task Handle(string message, HttpRequest request, WebSocket client)
    {
        if (message == null)
        {
            message = "";
        }

        if (message == "ping")
        {
            //兼容旧版client
            //如果是ping，回复本地数据的md5版本 
            var appId = request.Headers["appid"];
            var env = request.Headers["env"];
            env = await _configService.IfEnvEmptySetDefaultAsync(env);
            var md5 = await _configService.AppPublishedConfigsMd5CacheWithInheritanced(appId, env);
            await SendMessage(client, $"V:{md5}");
        }
        else
        {
            //如果无法处理，回复0
            await SendMessage(client, "0");
        }
    }
}