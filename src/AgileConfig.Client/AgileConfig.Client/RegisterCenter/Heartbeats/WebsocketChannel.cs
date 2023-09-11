using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgileConfig.Client.RegisterCenter.Heartbeats
{
    class WebsocketChannel : IChannel
    {
        private IConfigClient _client;
        private ILogger _logger;

        public WebsocketChannel(IConfigClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        private ClientWebSocket Websocket
        {
            get
            {
                return _client.WebSocket;
            }
        }

        public async Task SendAsync(string id)
        {
            if (Websocket.State == WebSocketState.Open)
            {
                try
                {
                    var msg = $"s:ping:{id}";
                    var data = Encoding.UTF8.GetBytes(msg);
                    await Websocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                            CancellationToken.None).ConfigureAwait(false);
                    _logger.LogTrace($"WebsocketChannel send a heartbeat message {msg} to server success .");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"WebsocketChannel send a heartbeat to server error .");
                }
            }
        }

    }
}
