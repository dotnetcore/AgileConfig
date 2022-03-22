using System;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public async Task Handle(string message, HttpRequest request, WebSocket client)
    {
        if (message == null)
        {
            message = "";
        }

        if (message == "ping")
        {
            //如果是ping，回复本地数据的md5版本 
            var appId = request.Headers["appid"];
            var env = request.Headers["env"];
            env = await _configService.IfEnvEmptySetDefaultAsync(env);
            var md5 = await _configService.AppPublishedConfigsMd5CacheWithInheritanced(appId, env);
            await SendMessage(client, JsonConvert.SerializeObject(new WebsocketAction()
            {
                Action = ActionConst.Ping,
                Module = ActionModule.ConfigCenter,
                Data = md5
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
                await SendMessage(client, JsonConvert.SerializeObject(new WebsocketAction()
                {
                    Action = ActionConst.Ping,
                    Module = ActionModule.RegisterCenter,
                    Data = version
                }));
            }
        }
        else
        {
            //如果无法处理，回复0
            await SendMessage(client, "0");
        }
    }
}