using Agile.Config.Protocol;
using AgileConfig.Server.Data.Entity;
using Newtonsoft.Json;
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

        public DateTime LastHeartbeatTime { get; set; }
    }

    public class WebsocketCollectionReport
    {
        public int ClientCount { get; set; }

        public List<WSClient> ClientsCopy { get; set; }
    }

    public interface IWebsocketCollection
    {
        WebsocketCollectionReport Report();

        WSClient Get(string clientId);
        void SendToAll(string message);
        Task SendToOne(WSClient client, string message);

        Task SendActionToOne(WSClient client, WebsocketAction action);
        void AddClient(WSClient client);

        Task RemoveClient(WSClient client, WebSocketCloseStatus? closeStatus, string closeDesc);

        void RemoveAppClients(string appId, WebSocketCloseStatus? closeStatus, string closeDesc);

        void SendActionToAppClients(string appId, WebsocketAction action);

        void SendActionToAll(WebsocketAction action);

        void SendToAppClients(string appId, string message);
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

        public void SendActionToAppClients(string appId, WebsocketAction action)
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
                var json = JsonConvert.SerializeObject(action);
                var data = Encoding.UTF8.GetBytes(json);
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

        public async Task SendActionToOne(WSClient client, WebsocketAction action)
        {
            if (client.Client.State == WebSocketState.Open)
            {
                var json = JsonConvert.SerializeObject(action);
                var data = Encoding.UTF8.GetBytes(json);
                await client.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
               CancellationToken.None);
            }
        }


        public void AddClient(WSClient client)
        {
            lock (_lockObj)
            {
                client.LastHeartbeatTime = DateTime.Now;
                Clients.Add(client);
            }
        }

        public async Task RemoveClient(WSClient client, WebSocketCloseStatus? closeStatus, string closeDesc = null)
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

        public WSClient Get(string clientId)
        {
            lock (_lockObj)
            {
                return Clients.FirstOrDefault(c => c.Id == clientId);
            }
        }

        public WebsocketCollectionReport Report()
        {
            lock (_lockObj)
            {
                return new WebsocketCollectionReport
                {
                    ClientCount = Clients.Count,
                    ClientsCopy = Clients
                                    .Select(c => new WSClient { Id = c.Id, AppId = c.AppId, LastHeartbeatTime = c.LastHeartbeatTime })
                                    .OrderBy(c => c.AppId)
                                    .ThenByDescending(c => c.LastHeartbeatTime)
                                    .ToList()
                };
            }
        }

        public void SendActionToAll(WebsocketAction action)
        {
            lock (_lockObj)
            {
                if (Clients.Count == 0)
                {
                    return;
                }

                var json = JsonConvert.SerializeObject(action);
                var data = Encoding.UTF8.GetBytes(json);
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

        private static IWebsocketCollection _Instance;
        public static IWebsocketCollection Instance
        {
            get
            {
                return _Instance ?? (_Instance = new WebsocketCollection());
            }
        }
    }

}