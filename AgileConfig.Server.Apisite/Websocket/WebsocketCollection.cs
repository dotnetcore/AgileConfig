using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Websocket
{
    public class WSClient
    {
        public WebSocket Client { get; set; }

        public string Id { get; set; }
    }

    public interface IWebsocketCollection
    {
        void SendToAll(string message);
        Task SendToOne(WSClient client, string message);
        void AddClient(WSClient client);

        Task CloseClient(WSClient client, WebSocketCloseStatus closeStatus, string closeDesc);
    }

    public class WebsocketCollection : IWebsocketCollection
    {
        private WebsocketCollection()
        {
        }

        private ConcurrentDictionary<string, WSClient> Clients = new ConcurrentDictionary<string, WSClient>();

        public void SendToAll(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            foreach (var webSocket in Clients.Values)
            {
                if (webSocket.Client.State == WebSocketState.Open)
                {
                    webSocket.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                 CancellationToken.None);
                }
            }
        }

        public async Task SendToOne(WSClient client, string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            if (client.Client.State == WebSocketState.Open)
                await client.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }

        public void AddClient(WSClient client)
        {
            Clients.TryAdd(client.Id, client);
        }

        public async Task CloseClient(WSClient client, WebSocketCloseStatus closeStatus, string closeDesc)
        {
            Clients.TryRemove(client.Id, out client);
            await client.Client.CloseAsync(closeStatus, closeDesc, CancellationToken.None);
            client.Client.Dispose();
        }

        private static WebsocketCollection _Instance;
        public static WebsocketCollection Instance
        {
            get
            {
                return _Instance ?? (_Instance = new WebsocketCollection());
            }
        }
    }
}