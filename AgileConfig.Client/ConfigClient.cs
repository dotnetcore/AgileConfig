using Agile.Config.Protocol;
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

namespace Agile.Config.Client
{
    public static class AgileConfig
    {
        private static ConfigClient _client;
        public static IConfigClient ClientInstance => _client;

        public static ILogger Logger { get; set; }
        public static string ServerNodes { get; set; }

        public static string AppId { get; set; }

        public static string Secret { get; set; }

        static AgileConfig()
        {
            _client = new ConfigClient();
        }
    }

    internal class ConfigClient : IConfigClient
    {
        private bool _isConnecting = false;
        private ClientWebSocket WebsocketClient { get; set; }
        private bool _adminSayOffline = false;

        private ConcurrentDictionary<string, string> _data;

        public event Action<ConfigChangedArg> ConfigChanged;

        public ConcurrentDictionary<string, string> Data => _data;


        internal ConfigClient()
        {
            _data = new ConcurrentDictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                Data.TryGetValue(key, out string val);
                return val;
            }
        }

        private string PickOneServer()
        {
            if (string.IsNullOrEmpty(AgileConfig.ServerNodes))
            {
                throw new ArgumentNullException("ServerNodes");
            }

            var servers = AgileConfig.ServerNodes.Split(',');
            if (servers.Length == 1)
            {
                return servers[0];
            }

            var index = new Random().Next(0, servers.Length);

            return servers[index];
        }

        /// <summary>
        /// 开启一个线程来初始化Websocket Client，并且5s一次进行检查是否连接打开状态，如果不是则尝试重连。
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            if (_isConnecting)
            {
                return;
            }
            _isConnecting = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    if (WebsocketClient?.State == WebSocketState.Open)
                    {
                        await Task.Delay(1000 * 5);

                        continue;
                    }
                    try
                    {
                        WebsocketClient?.Abort();
                        WebsocketClient?.Dispose();
                        if (_adminSayOffline)
                        {
                            break;
                        }
                        WebsocketClient = null;
                        WebsocketClient = new ClientWebSocket();
                        WebsocketClient.Options.SetRequestHeader("appid", AgileConfig.AppId);
                        WebsocketClient.Options.SetRequestHeader("Authorization", GenerateBasicAuthorization(AgileConfig.AppId, AgileConfig.Secret));
                        var server = PickOneServer();
                        var websocketServerUrl = "";
                        if (server.StartsWith("https:", StringComparison.CurrentCultureIgnoreCase))
                        {
                            websocketServerUrl = server.Replace("https:", "wss:").Replace("HTTPS:", "wss:");
                        }
                        else
                        {
                            websocketServerUrl = server.Replace("http:", "ws:").Replace("HTTP:", "ws:");
                        }
                        websocketServerUrl += "/ws";
                        await WebsocketClient.ConnectAsync(new Uri(websocketServerUrl), CancellationToken.None);
                        AgileConfig.Logger?.LogTrace("AgileConfig Client Websocket Connected , {0}", websocketServerUrl);
                        HandleWebsocketMessageAsync();
                        WebsocketHeartbeatAsync();
                        //连接成功重新加载配置
                        Load();
                    }
                    catch (Exception ex)
                    {
                        AgileConfig.Logger?.LogError(ex, "AgileConfig Client Websocket try to connected to server failed.");
                    }
                    await Task.Delay(1000 * 5);
                }
            });
        }

        private string GenerateBasicAuthorization(string appId, string secret)
        {
            var txt = $"{appId}:{secret}";
            var data = Encoding.UTF8.GetBytes(txt);
            return "Basic " + Convert.ToBase64String(data);
        }
        /// <summary>
        /// 开启一个线程5s进行一次心跳
        /// </summary>
        /// <returns></returns>
        public void WebsocketHeartbeatAsync()
        {
            Task.Run(async () =>
            {
                var data = Encoding.UTF8.GetBytes("hi");
                while (true)
                {
                    await Task.Delay(1000 * 5);
                    if (_adminSayOffline)
                    {
                        break;
                    }
                    if (WebsocketClient?.State == WebSocketState.Open)
                    {
                        try
                        {
                            //这里由于多线程的问题，WebsocketClient有可能在上一个if判断成功后被置空或者断开，所以需要try一下避免线程退出
                            await WebsocketClient.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                                    CancellationToken.None);
                            AgileConfig.Logger?.LogTrace("AgileConfig Client Say 'hi' by Websocket .");
                        }
                        catch (Exception ex)
                        {
                            AgileConfig.Logger?.LogError(ex, "AgileConfig Client Websocket try to send Heartbeat to server failed.");
                        }
                    }
                }
            });
        }
        /// <summary>
        /// 开启一个线程对服务端推送的websocket message进行处理
        /// </summary>
        /// <returns></returns>
        private void HandleWebsocketMessageAsync()
        {
            Task.Run(async () =>
            {
                while (WebsocketClient?.State == WebSocketState.Open)
                {
                    ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[1024 * 2]);
                    WebSocketReceiveResult result = null;
                    try
                    {
                        result = await WebsocketClient.ReceiveAsync(buffer, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        AgileConfig.Logger?.LogError(ex, "AgileConfig Client Websocket try to ReceiveAsync message occur exception .");
                        throw;
                    }
                    if (result.CloseStatus.HasValue)
                    {
                        AgileConfig.Logger?.LogTrace("AgileConfig Client Websocket closed , {0} .", result.CloseStatusDescription);
                        break;
                    }
                    ProcessMessage(result, buffer);
                }
            });
        }

        /// <summary>
        /// 最终处理服务端推送的消息
        /// </summary>
        private async void ProcessMessage(WebSocketReceiveResult result, ArraySegment<Byte> buffer)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(buffer.Array, buffer.Offset, result.Count);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        var msg = await reader.ReadToEndAsync();
                        AgileConfig.Logger?.LogTrace("AgileConfig Client Receive message ' {0} ' by Websocket .", msg);
                        if (string.IsNullOrEmpty(msg))
                        {
                            return;
                        }
                        if (msg == "hi")
                        {
                            return;
                        }
                        try
                        {
                            var action = JsonConvert.DeserializeObject<WebsocketAction>(msg);
                            if (action != null)
                            {
                                var dict = Data as ConcurrentDictionary<string, string>;
                                switch (action.Action)
                                {
                                    case ActionConst.Add:
                                        NoticeChangedAsync(ActionConst.Add, GenerateKey(action.Item));
                                        break;
                                    case ActionConst.Update:
                                        var key = GenerateKey(action.Item);
                                        if (action.OldItem != null)
                                        {
                                            dict.TryRemove(GenerateKey(action.OldItem), out string oldV);
                                        }
                                        dict.AddOrUpdate(key, action.Item.value, (k, v) => { return action.Item.value; });
                                        NoticeChangedAsync(ActionConst.Update, key);
                                        break;
                                    case ActionConst.Remove:
                                        var key1 = GenerateKey(action.Item);
                                        dict.TryRemove(key1, out string oldV1);
                                        NoticeChangedAsync(ActionConst.Remove, key1);
                                        break;
                                    case ActionConst.Offline:
                                        _adminSayOffline = true;
                                        await WebsocketClient.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                                        AgileConfig.Logger?.LogTrace("Websocket client offline because admin console send a commond 'offline' ,");
                                        NoticeChangedAsync(ActionConst.Offline);
                                        break;
                                    case ActionConst.Reload:
                                        Load();
                                        NoticeChangedAsync(ActionConst.Reload);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AgileConfig.Logger?.LogError(ex, "Cannot handle websocket message {0}", msg);
                        }
                    }
                }
            }
        }

        private Task NoticeChangedAsync(string action, string key = "")
        {
            if (ConfigChanged == null)
            {
                return Task.CompletedTask;
            }
            return Task.Run(() =>
            {
                ConfigChanged(new ConfigChangedArg(action, key));
            });
        }

        private string GenerateKey(ConfigItem item)
        {
            var key = new StringBuilder();
            if (!string.IsNullOrEmpty(item.group))
            {
                key.Append(item.group + ":");
            }
            key.Append(item.key);

            return key.ToString();
        }

        /// <summary>
        /// 通过http从server拉取所有配置到本地，尝试5次
        /// </summary>
        public bool Load()
        {
            int tryCount = 0;
            while (tryCount <= 4)
            {
                tryCount++;

                try
                {
                    var op = new AgileHttp.RequestOptions()
                    {
                        Headers = new Dictionary<string, string>()
                        {
                            {"appid", AgileConfig.AppId },
                            {"Authorization", GenerateBasicAuthorization(AgileConfig.AppId,AgileConfig.Secret) }
                        }
                    };
                    var apiUrl = $"{PickOneServer()}/api/config/app/{AgileConfig.AppId}";
                    using (var result = AgileHttp.HTTP.Send(apiUrl, "GET", null, op))
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Data.Clear();
                            var concurrentDict = Data as ConcurrentDictionary<string, string>;
                            var configs = JsonConvert.DeserializeObject<List<ConfigItem>>(result.GetResponseContent());
                            configs.ForEach(c =>
                            {
                                var key = GenerateKey(c);
                                string value = c.value;
                                concurrentDict.TryAdd(key.ToString(), value);
                            });
                            AgileConfig.Logger?.LogTrace("AgileConfig Client Loaded all the configs success from {0} , Try count: {1}.", apiUrl, tryCount);
                            return true;
                        }
                        else
                        {
                            //load remote configs err .
                            var ex = result.Exception ?? new Exception("AgileConfig Client Load all the configs failed .");
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AgileConfig.Logger?.LogError(ex, "AgileConfig Client try to load all configs failed . TryCount: {0}", tryCount);
                }
            }

            return false;
        }

    }
}
