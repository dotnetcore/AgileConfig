using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public string AppId { get; set; }
    }

    public interface IWebsocketCollection
    {
        void SendToAll(string message);
        Task SendToOne(WSClient client, string message);
        void AddClient(WSClient client);

        Task RemoveClient(WSClient client, WebSocketCloseStatus? closeStatus, string closeDesc);

        void RemoveAppClients(string appId, WebSocketCloseStatus? closeStatus, string closeDesc);
    }

    public class WebsocketCollection : IWebsocketCollection
    {
        private WebsocketCollection()
        {
        }

        private List<WSClient> Clients = new List<WSClient>();
        private object _lockObj = new object();

        public void SendToAll(string message)
        {
            lock (_lockObj)
            {
                if (Clients.Count == 0)
                {
                    return;
                }
                var data = Encoding.UTF8.GetBytes(message);
                foreach (var webSocket in Clients)
                {
                    if (webSocket.Client.State == WebSocketState.Open)
                    {
                        webSocket.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                     CancellationToken.None);
                    }
                }
            }
        }

        public void SendToAppClients(string appId, string message)
        {
            lock (_lockObj)
            {
                if (Clients.Count == 0)
                {
                    return;
                }
                var appClients = Clients.Where(c => c.AppId == appId);
                if (appClients.Count() == 0)
                {
                    return;
                }
                var data = Encoding.UTF8.GetBytes(message);
                foreach (var webSocket in appClients)
                {
                    if (webSocket.AppId == appId && webSocket.Client.State == WebSocketState.Open)
                    {
                        webSocket.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                     CancellationToken.None);
                    }
                }
            }
        }

        public async Task SendToOne(WSClient client, string message)
        {
            if (client.Client.State == WebSocketState.Open)
            {
                var data = Encoding.UTF8.GetBytes(message);
                await client.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
               CancellationToken.None);
            }
        }

        public void AddClient(WSClient client)
        {
            lock (_lockObj)
            {
                Clients.Add(client);
            }
        }

        public async Task RemoveClient(WSClient client, WebSocketCloseStatus? closeStatus, string closeDesc)
        {
            lock (_lockObj)
            {
                Clients.Remove(client);
            }
            if (client.Client.State == WebSocketState.Open)
            {
                await client.Client.CloseAsync(closeStatus.HasValue ? closeStatus.Value : WebSocketCloseStatus.Empty, closeDesc, CancellationToken.None);
                client.Client.Dispose();
            }
        }

        public void RemoveAppClients(string appId, WebSocketCloseStatus? closeStatus, string closeDesc)
        {
            lock (_lockObj)
            {
                var removeClients = Clients.Where(c => c.AppId == appId).ToList();
                if (removeClients.Count == 0)
                {
                    return;
                }
                foreach (var webSocket in removeClients)
                {
                    Clients.Remove(webSocket);
                }
                Task.Run(async () =>
                {
                    foreach (var webSocket in removeClients)
                    {
                        try
                        {
                            if (webSocket.Client.State == WebSocketState.Open)
                            {
                                await webSocket.Client.CloseAsync(closeStatus.HasValue ? closeStatus.Value : WebSocketCloseStatus.Empty, closeDesc, CancellationToken.None);
                                webSocket.Client.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Try to close websocket client {0} err {1}.", webSocket.Id, ex.Message);
                        }
                    }
                });
            }
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