using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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
            _logger = loggerFactory.
                CreateLogger<WebsocketHandlerMiddleware>();
            _websocketCollection = WebsocketCollection.Instance;
        }

        public async Task Invoke(HttpContext context, IAppBasicAuthService appBasicAuth, IConfigService configService)
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
                    context.Request.Query.TryGetValue("client_name", out StringValues name);
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = HttpUtility.UrlDecode(name);
                    }
                    else
                    {
                        _logger.LogInformation("Websocket client request No Name property ");
                    }
                    context.Request.Query.TryGetValue("client_tag", out StringValues tag);
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tag = HttpUtility.UrlDecode(tag);
                    }
                    else
                    {
                        _logger.LogInformation("Websocket client request No TAG property ");
                    }
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                   
                    var clientIp = GetRemoteIp(context.Request);
                    var client = new WebsocketClient()
                    {
                        Client = webSocket,
                        Id = Guid.NewGuid().ToString(),
                        AppId = appId,
                        LastHeartbeatTime = DateTime.Now,
                        Name = name,
                        Tag = tag,
                        Ip = clientIp.ToString()
                    };
                    _websocketCollection.AddClient(client);
                    _logger.LogInformation("Websocket client {0} Added ", client.Id);

                    try
                    {
                        await Handle(context, client, configService);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Handle websocket client {0} err .", client.Id);
                        await _websocketCollection.RemoveClient(client, WebSocketCloseStatus.Empty, ex.Message);
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

        public IPAddress GetRemoteIp(HttpRequest httpRequest)
        {
            IPAddress ip;
            var headers = httpRequest.Headers.ToList();
            if (headers.Exists((kvp) => kvp.Key == "X-Forwarded-For"))
            {
                // when running behind a load balancer you can expect this header
                var header = headers.First((kvp) => kvp.Key == "X-Forwarded-For").Value.ToString();
                IPAddress.TryParse(header, out ip);
            }
            else
            {
                // this will always have a value (running locally in development won't have the header)
                ip = httpRequest.HttpContext.Connection.RemoteIpAddress;
            }

            return ip;
        }

        private async Task Handle(HttpContext context, WebsocketClient socketClient, IConfigService configService)
        {
            var buffer = new byte[1024 * 2];
            WebSocketReceiveResult result = null;
            do
            {
                result = await socketClient.Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                socketClient.LastHeartbeatTime = DateTime.Now;
                var message = await ReadWebsocketMessage(result, buffer);
                if (message == "ping")
                {
                    //如果是ping，回复本地数据的md5版本
                    var appId = context.Request.Headers["appid"];
                    var env = context.Request.Headers["env"];
                    if (string.IsNullOrEmpty(env))
                    {
                        env = "DEV";
                    }
                    var md5 = await configService.AppPublishedConfigsMd5CacheWithInheritanced(appId, env);
                    await SendMessage(socketClient.Client, $"V:{md5}");
                }
                else
                {
                    //如果不是心跳消息，回复0
                    await SendMessage(socketClient.Client, "0");
                }
            }
            while (!result.CloseStatus.HasValue);
            _logger.LogInformation($"Websocket close , closeStatus:{result.CloseStatus} closeDesc:{result.CloseStatusDescription}");
            await _websocketCollection.RemoveClient(socketClient, result.CloseStatus, result.CloseStatusDescription);
        }

        private async Task SendMessage(WebSocket webSocket, string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, CancellationToken.None);
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