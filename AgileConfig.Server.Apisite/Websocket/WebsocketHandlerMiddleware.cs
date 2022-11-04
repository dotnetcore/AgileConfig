using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Agile.Config.Protocol;
using AgileConfig.Server.Apisite.Websocket.MessageHandlers;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Websocket
{
    public class WebsocketHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IWebsocketCollection _websocketCollection;

        public WebsocketHandlerMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory
        )
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<WebsocketHandlerMiddleware>();
            _websocketCollection = WebsocketCollection.Instance;
        }

        public async Task Invoke(
            HttpContext context,
            IAppBasicAuthService appBasicAuth,
            IConfigService configService,
            IRegisterCenterService registerCenterService,
            IServiceInfoService serviceInfoService)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    if (!await appBasicAuth.ValidAsync(context.Request))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("basic auth failed .");
                        return;
                    }

                    var appId = context.Request.Headers["appid"];
                    if (string.IsNullOrEmpty(appId))
                    {
                        var appIdSecret = appBasicAuth.GetAppIdSecret(context.Request);
                        appId = appIdSecret.Item1;
                    }

                    var env = context.Request.Headers["env"];
                    if (!string.IsNullOrEmpty(env))
                    {
                        env = HttpUtility.UrlDecode(env);
                    }
                    else
                    {
                        env = "DEV";
                        _logger.LogInformation("Websocket client request No ENV property , set default DEV ");
                    }

                    context.Request.Query.TryGetValue("client_name", out StringValues name);
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = HttpUtility.UrlDecode(name);
                    }

                    context.Request.Query.TryGetValue("client_tag", out StringValues tag);
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tag = HttpUtility.UrlDecode(tag);
                    }

                    DateTime lastRefreshTime = new DateTime(1970, 1, 1);
                    //string strClientLastRefreshTime = context.Request.Headers["client_last_refresh_time"];
                    //if (!String.IsNullOrWhiteSpace(strClientLastRefreshTime))
                    //{
                    //    strClientLastRefreshTime = HttpUtility.UrlDecode(strClientLastRefreshTime);
                    //    DateTime.TryParse(strClientLastRefreshTime, out lastRefreshTime);
                    //}

                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    var clientIp = Common.HttpExt.GetRemoteIp(context.Request);
                    var client = new WebsocketClient()
                    {
                        Client = webSocket,
                        Id = Guid.NewGuid().ToString(),
                        AppId = appId,
                        LastHeartbeatTime = DateTime.Now,
                        Name = name,
                        Tag = tag,
                        Ip = clientIp.ToString(),
                        Env = env,
                        LastRefreshTime = lastRefreshTime,
                    };
                    _websocketCollection.AddClient(client);
                    _logger.LogInformation("Websocket client {0} Added ", client.Id);

                    try
                    {
                        await Handle(context, client, configService, registerCenterService, serviceInfoService);
                    }
                    catch (WebSocketException)
                    {
                        _logger.LogInformation("client {0} closed the websocket connection directly .", client.Id);
                        await _websocketCollection.RemoveClient(client, WebSocketCloseStatus.Empty, null);
                        await context.Response.WriteAsync("500 closed");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Handle websocket client {0} err .", client.Id);
                        await _websocketCollection.RemoveClient(client, WebSocketCloseStatus.Empty, null);
                        await context.Response.WriteAsync("500 closed");
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }

        /// <summary>
        /// 对client的消息进行处理
        /// ，如果是ping是老版client的心跳消息
        /// ，如果是c:打头的消息代表是配置中心的client的消息
        /// ，如果是s:打头的消息代表是服务中心的client的消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socketClient"></param>
        /// <param name="configService"></param>
        /// <param name="registerCenterService"></param>
        private async Task Handle(
            HttpContext context,
            WebsocketClient socketClient,
            IConfigService configService,
            IRegisterCenterService registerCenterService,
            IServiceInfoService serviceInfoService)
        {
            var messageHandlers =
                new WebsocketMessageHandlers(configService, registerCenterService, serviceInfoService);
            var buffer = new byte[1024 * 2];
            WebSocketReceiveResult result = null;
            do
            {
                result = await socketClient.Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.CloseStatus.HasValue)
                {
                    break;
                }
                socketClient.LastHeartbeatTime = DateTime.Now;
                var message = await ReadWebsocketMessage(result, buffer);

                //配置中心 ping 带参数
                if (!String.IsNullOrWhiteSpace(message) && message.StartsWith("ping:"))
                {
                    string infoModel = message.Substring(5);
                    if (!String.IsNullOrWhiteSpace(infoModel))
                    {
                        try
                        {
                            ClientInfo clientInfo = JsonConvert.DeserializeObject<ClientInfo>(infoModel);
                            if (clientInfo != null)
                            {
                                socketClient.LastRefreshTime = clientInfo.LastRefreshTime;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                $"解析配置中心客户端ping参数失败, message: {message}");
                        }
                    }
                }

                foreach (var messageHandlersMessageHandler in messageHandlers.MessageHandlers)
                {
                    if (messageHandlersMessageHandler.Hit(context.Request))
                    {
                        await messageHandlersMessageHandler.Handle(message, context.Request, socketClient.Client);
                    }
                }
            } while (!result.CloseStatus.HasValue);

            _logger.LogInformation(
                $"Websocket close , closeStatus:{result.CloseStatus} closeDesc:{result.CloseStatusDescription}");
            await _websocketCollection.RemoveClient(socketClient, result.CloseStatus, result.CloseStatusDescription);
        }

        private async Task<string> ReadWebsocketMessage(WebSocketReceiveResult result, ArraySegment<Byte> buffer)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(buffer.Array, buffer.Offset, result.Count);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }

                return "";
            }
        }
    }
}