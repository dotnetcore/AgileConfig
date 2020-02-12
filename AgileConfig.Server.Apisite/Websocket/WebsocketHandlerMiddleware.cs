using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        public async Task Invoke(HttpContext context, IAppService appService)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var basicAuth = new BasicAuthenticationAttribute(appService);
                    if (!await basicAuth.Valid(context.Request))
                    {
                        await context.Response.WriteAsync("closed");
                        return;
                    }
                    var appId = context.Request.Headers["appid"];
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var client = new WSClient()
                    {
                        Client = webSocket,
                        Id = Guid.NewGuid().ToString(),
                        AppId = appId
                    };
                    _websocketCollection.AddClient(client);
                    _logger.LogInformation("Websocket client {0} Added ", client.Id);
                    try
                    {
                        await Echo(context, client);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Echo websocket client {0} err .", client.Id);
                        await _websocketCollection.RemoveClient(client, WebSocketCloseStatus.Empty, ex.Message);
                        await context.Response.WriteAsync("closed");
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

        private async Task Echo(HttpContext context, WSClient webSocket)
        {
            var buffer = new byte[1024 * 2];
            WebSocketReceiveResult result = await webSocket.Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            webSocket.LastHeartbeatTime = DateTime.Now;
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.Client.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await webSocket.Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                webSocket.LastHeartbeatTime = DateTime.Now;
            }
            _logger.LogInformation($"Websocket close , closeStatus:{result.CloseStatus} closeDesc:{result.CloseStatusDescription}");
            await _websocketCollection.RemoveClient(webSocket, result.CloseStatus, result.CloseStatusDescription);
        }
    }
}