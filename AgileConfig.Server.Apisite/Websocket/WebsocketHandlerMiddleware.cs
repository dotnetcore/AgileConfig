using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.
                CreateLogger<WebsocketHandlerMiddleware>();
            _websocketCollection = WebsocketCollection.Instance;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var client = new WSClient()
                    {
                        Client = webSocket,
                        Id = Guid.NewGuid().ToString()
                    };
                    _websocketCollection.AddClient(client);
                    await Echo(context, client);
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
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.Client.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await webSocket.Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            _logger.LogInformation($"Websocket close , closeStatus:{webSocket.Client.CloseStatus} closeDesc:{webSocket.Client.CloseStatusDescription}");
            await _websocketCollection.CloseClient(webSocket, result.CloseStatus.Value, result.CloseStatusDescription);
        }
    }
}