using Agile.Config.Protocol;
using AgileConfig.Server.IService;
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
    public class WebsocketCollection : IWebsocketCollection
    {
        private WebsocketCollection()
        {
        }

        static WebsocketCollection()
        {
            Instance = new WebsocketCollection();
        }

        private ConcurrentDictionary<string, WebsocketClient> _Clients = new ConcurrentDictionary<string, WebsocketClient>();

        public void SendMessageToAll(string message)
        {
            if (_Clients.Count == 0)
            {
                return;
            }
            var data = Encoding.UTF8.GetBytes(message);
            foreach (var webSocket in _Clients)
            {
                if (webSocket.Value.Client.State == WebSocketState.Open)
                {
                    webSocket.Value.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                 CancellationToken.None);
                }
            }
        }

        public void SendToAppClients(string appId, string message)
        {
            if (_Clients.Count == 0)
            {
                return;
            }
            var appClients = _Clients.Values.Where(c => c.AppId == appId);
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

        public void SendActionToAppClients(string appId, WebsocketAction action)
        {
            if (_Clients.Count == 0)
            {
                return;
            }
            var appClients = _Clients.Values.Where(c => c.AppId == appId);
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


        public async Task SendMessageToOne(WebsocketClient client, string message)
        {
            if (client.Client.State == WebSocketState.Open)
            {
                var data = Encoding.UTF8.GetBytes(message);
                await client.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
               CancellationToken.None);
            }
        }

        public async Task SendActionToOne(WebsocketClient client, WebsocketAction action)
        {
            if (client.Client.State == WebSocketState.Open)
            {
                var json = JsonConvert.SerializeObject(action);
                var data = Encoding.UTF8.GetBytes(json);
                await client.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
               CancellationToken.None);
            }
        }


        public void AddClient(WebsocketClient client)
        {
            _Clients.TryAdd(client.Id, client);
        }

        public async Task RemoveClient(WebsocketClient client, WebSocketCloseStatus? closeStatus, string closeDesc = null)
        {
            if (_Clients.TryRemove(client.Id, out WebsocketClient tryRemoveClient) && client.Client.State == WebSocketState.Open)
            {
                await client.Client.CloseAsync(closeStatus.HasValue ? closeStatus.Value : WebSocketCloseStatus.Empty, closeDesc, CancellationToken.None);
                client.Client.Dispose();
            }
        }

        public void RemoveAppClients(string appId, WebSocketCloseStatus? closeStatus, string closeDesc)
        {
            var removeClients = _Clients.Values.Where(c => c.AppId == appId).ToList();
            if (removeClients.Count == 0)
            {
                return;
            }
            foreach (var webSocket in removeClients)
            {
                _Clients.TryRemove(webSocket.Id, out WebsocketClient tryRemoveClient);
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

        public WebsocketClient Get(string clientId)
        {
            _Clients.TryGetValue(clientId, out WebsocketClient client);
            return client;
        }

        public ClientInfos Report()
        {
            var clientInfos = _Clients
                                  .Values
                                  .Select(c => new ClientInfo { Id = c.Id, AppId = c.AppId, LastHeartbeatTime = c.LastHeartbeatTime })
                                  .OrderBy(c => c.AppId)
                                  .ThenByDescending(c => c.LastHeartbeatTime)
                                  .ToList();
            return new ClientInfos
            {
                ClientCount = clientInfos.Count,
                Infos = clientInfos
            };
        }

        public void SendActionToAll(WebsocketAction action)
        {
            if (_Clients.Count == 0)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(action);
            var data = Encoding.UTF8.GetBytes(json);
            foreach (var webSocket in _Clients)
            {
                if (webSocket.Value.Client.State == WebSocketState.Open)
                {
                    webSocket.Value.Client.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                 CancellationToken.None);
                }
            }
        }

        public static IWebsocketCollection Instance { get; private set; }

        public int Count => _Clients.Count;
    }

}