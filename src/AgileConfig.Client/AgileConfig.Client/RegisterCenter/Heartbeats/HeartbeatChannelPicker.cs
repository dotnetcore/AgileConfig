using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Client.RegisterCenter.Heartbeats
{
    public class HeartbeatChannelPicker
    {
        private ILoggerFactory _loggerFactory;
        private IConfigClient _client;

        private IChannel _wsChan;
        private IChannel _httpChan;

        public HeartbeatChannelPicker(IConfigClient client, ILoggerFactory loggerFactory)
        {
            _client = client;
            _loggerFactory = loggerFactory;
            _wsChan = new WebsocketChannel(_client, _loggerFactory.CreateLogger<WebsocketChannel>());
            _httpChan = new HttpChannel(_client.Options, _loggerFactory.CreateLogger<HttpChannel>());
        }

        public IChannel Pick()
        {
            if (_client != null && _client.WebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                return _wsChan;
            }

            return _httpChan;
        }
    }
}
