using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgileConfig.Client.Configuration
{
    internal class AgileConfigNode
    {
        public string key { get; set; }

        public string value { get; set; }

        public string group { get; set; }
    }

    internal class WebsocketAction
    {
        public string Action { get; set; }

        public AgileConfigNode Node { get; set; }

        public AgileConfigNode OldNode { get; set; }
    }

    public class AgileConfigProvider : ConfigurationProvider
    {
        protected ILogger Logger { get; }

        protected string Host { get; }

        protected string AppId { get; }

        protected string Secret { get; }

        ClientWebSocket WebsocketClient { get; }

        protected string WSUrl { get; }

        public AgileConfigProvider(string host, string appId, string secret, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }
            Logger = loggerFactory?.
             CreateLogger<AgileConfigProvider>();
            Host = host;
            AppId = appId;
            Secret = secret;

            if (host.StartsWith("https:", StringComparison.CurrentCultureIgnoreCase))
            {
                WSUrl = host.Replace("https:", "wss:").Replace("HTTPS:", "wss:");
            }
            else
            {
                WSUrl = host.Replace("http:", "ws:").Replace("HTTP:", "ws:");
            }
            WSUrl += "/ws";
            WebsocketClient = new ClientWebSocket();
        }

        private async Task WebsocketConnect()
        {
            await WebsocketClient.ConnectAsync(new Uri(WSUrl), CancellationToken.None);
            Logger?.LogTrace("AgileConfig Client Websocket Connected.");
            HandleWebsocketMessageAsync();
            WebsocketHeartbeatAsync();
        }
        private Task WebsocketHeartbeatAsync()
        {
            return Task.Run(async () =>
            {
                var data = Encoding.UTF8.GetBytes("hi");
                while (true)
                {
                    await Task.Delay(1000 * 5);
                    if (WebsocketClient.State == WebSocketState.Open)
                    {
                        await WebsocketClient.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                        CancellationToken.None);
                        Logger?.LogTrace("AgileConfig Client Say 'hi' by Websocket .");
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }

        private Task HandleWebsocketMessageAsync()
        {
            return Task.Run(async () =>
            {
                while (WebsocketClient.State == WebSocketState.Open)
                {
                    ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[1024 * 2]);
                    WebSocketReceiveResult result = null;
                    result = await WebsocketClient.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.CloseStatus.HasValue)
                    {
                        Logger?.LogTrace("AgileConfig Client Websocket closed , {0} .", result.CloseStatusDescription);
                        break;
                    }
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            using (var reader = new StreamReader(ms, Encoding.UTF8))
                            {
                                var msg = await reader.ReadToEndAsync();
                                Logger?.LogTrace("AgileConfig Client Receive message ' {0} ' by Websocket .", msg);
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    if (msg == "hi")
                                    {
                                        continue;
                                    }
                                    try
                                    {
                                        var action = JsonConvert.DeserializeObject<WebsocketAction>(msg);
                                        if (action != null)
                                        {
                                            var dict = Data as ConcurrentDictionary<string, string>;
                                            switch (action.Action)
                                            {
                                                case "update":
                                                case "add":
                                                    var key = GenerateKey(action.Node);
                                                    if (action.OldNode != null)
                                                    {
                                                        dict.TryRemove(GenerateKey(action.OldNode), out string oldV);
                                                    }
                                                    dict.AddOrUpdate(key, action.Node.value, (k, v) => { return action.Node.value; });
                                                    break;
                                                case "remove":
                                                    dict.TryRemove(GenerateKey(action.Node), out string oldV1);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger?.LogError(ex, "Cannot handle websocket message {0}", msg);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        private string GenerateKey(AgileConfigNode node)
        {
            var key = new StringBuilder();
            if (!string.IsNullOrEmpty(node.group))
            {
                key.Append(node.group + ":");
            }
            key.Append(node.key);

            return key.ToString();
        }

        public async override void Load()
        {
            var apiUrl = $"{Host}/api/config/app/{AppId}";
            using (var result = AgileHttp.HTTP.Send(apiUrl))
            {
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Data = new ConcurrentDictionary<string, string>();
                    var configs = JsonConvert.DeserializeObject<List<AgileConfigNode>>(result.GetResponseContent());
                    configs.ForEach(c =>
                    {
                        var key = GenerateKey(c);
                        string value = c.value;

                        var dict = Data as ConcurrentDictionary<string, string>;
                        dict.TryAdd(key.ToString(), value);
                    });
                    Logger?.LogTrace("AgileConfig Client Loaded all the configs success by http {0} .", apiUrl);
                    await WebsocketConnect();
                }
                else
                {
                    //load remote configs err .
                    var errMsg = "AgileConfig Client Load all the configs failed .";
                    var ex = new Exception(errMsg, result.Exception);
                    Logger?.LogError(ex, errMsg, apiUrl);
                    throw ex;
                }
            }
        }
    }
}
