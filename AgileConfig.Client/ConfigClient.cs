using Agile.Config.Protocol;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Agile.Config.Client
{
    public class ConfigClient : IConfigClient
    {
        public ConfigClient(string appId, string secret, string serverNodes, ILogger logger = null)
        {
            this.Logger = logger;
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }
            if (string.IsNullOrEmpty(serverNodes))
            {
                throw new ArgumentNullException(nameof(serverNodes));
            }
            this.AppId = appId;
            this.Secret = secret;
            this.ServerNodes = serverNodes;
            _data = new ConcurrentDictionary<string, string>();
        }

        private int WebsocketReconnectInterval = 5;
        private int WebsocketHeartbeatInterval = 30;

        public event Action<ConfigChangedArg> ConfigChanged;
        public ConcurrentDictionary<string, string> Data => _data;
        public ILogger Logger { get; set; }
        private string ServerNodes { get; set; }
        private string AppId { get; set; }
        private string Secret { get; set; }
        private bool _isAutoReConnecting = false;
        private ClientWebSocket WebsocketClient { get; set; }
        private bool _adminSayOffline = false;
        private ConcurrentDictionary<string, string> _data;

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
            if (string.IsNullOrEmpty(ServerNodes))
            {
                throw new ArgumentNullException("ServerNodes");
            }

            var servers = ServerNodes.Split(',');
            if (servers.Length == 1)
            {
                return servers[0];
            }

            var index = new Random().Next(0, servers.Length);

            return servers[index];
        }

        public async Task<bool> ConnectAsync()
        {
            if (WebsocketClient?.State == WebSocketState.Open)
            {
                return true;
            }

            if (WebsocketClient == null)
            {
                WebsocketClient = new ClientWebSocket();
            }
            var connected = await TryConnectWebsocketAsync().ConfigureAwait(false);
            if (connected)
            {
                //连接成功立马加载配置
                Load();
                HandleWebsocketMessageAsync();
                WebsocketHeartbeatAsync();
                //设置自动重连
                AutoReConnect();
            }

            return connected;
        }

        private async Task<bool> TryConnectWebsocketAsync()
        {
            WebsocketClient.Options.SetRequestHeader("appid", AppId);
            WebsocketClient.Options.SetRequestHeader("Authorization", GenerateBasicAuthorization(AppId, Secret));
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
            await WebsocketClient.ConnectAsync(new Uri(websocketServerUrl), CancellationToken.None).ConfigureAwait(false);
            Logger?.LogTrace("AgileConfig Client Websocket Connected , {0}", websocketServerUrl);

            return true;
        }

        /// <summary>
        /// 开启一个线程来初始化Websocket Client，并且5s一次进行检查是否连接打开状态，如果不是则尝试重连。
        /// </summary>
        /// <returns></returns>
        private void AutoReConnect()
        {
            if (_isAutoReConnecting)
            {
                return;
            }
            _isAutoReConnecting = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000 * WebsocketReconnectInterval).ConfigureAwait(false);

                    if (WebsocketClient?.State == WebSocketState.Open)
                    {
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

                        WebsocketClient = new ClientWebSocket();
                        var connected = await TryConnectWebsocketAsync().ConfigureAwait(false);
                        if (connected)
                        {
                            Load();
                            HandleWebsocketMessageAsync();
                            WebsocketHeartbeatAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "AgileConfig Client Websocket try to connected to server failed.");
                    }
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
        /// 开启一个线程30s进行一次心跳
        /// </summary>
        /// <returns></returns>
        public void WebsocketHeartbeatAsync()
        {
            Task.Run(async () =>
            {
                var data = Encoding.UTF8.GetBytes("ping");
                while (true)
                {
                    await Task.Delay(1000 * WebsocketHeartbeatInterval).ConfigureAwait(false); ;
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
                                    CancellationToken.None).ConfigureAwait(false);
                            Logger?.LogTrace("AgileConfig Client Say 'hi' by Websocket .");
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogError(ex, "AgileConfig Client Websocket try to send Heartbeat to server failed.");
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
                        result = await WebsocketClient.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "AgileConfig Client Websocket try to ReceiveAsync message occur exception .");
                        throw;
                    }
                    if (result != null && result.CloseStatus.HasValue)
                    {
                        Logger?.LogTrace("AgileConfig Client Websocket closed , {0} .", result.CloseStatusDescription);
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
                        var msg = await reader.ReadToEndAsync().ConfigureAwait(false);
                        Logger?.LogTrace("AgileConfig Client Receive message ' {0} ' by Websocket .", msg);
                        if (string.IsNullOrEmpty(msg) || msg == "0")
                        {
                            return;
                        }
                        if (msg.StartsWith("V:"))
                        {
                            var version = msg.Substring(2, msg.Length - 2);
                            var localVersion = this.DataMd5Version();
                            if (version != localVersion)
                            {
                                //如果数据库版本跟本地版本不一致则直接全部更新
                                Load();
                            }
                            return;
                        }
                        try
                        {
                            var action = JsonConvert.DeserializeObject<WebsocketAction>(msg);
                            if (action != null)
                            {
                                var dict = Data as ConcurrentDictionary<string, string>;
                                var itemKey = "";
                                if (action.Item != null)
                                {
                                    itemKey = GenerateKey(action.Item);
                                }
                                switch (action.Action)
                                {
                                    case ActionConst.Add:
                                        dict.AddOrUpdate(itemKey, action.Item.value, (k, v) => { return action.Item.value; });
                                        NoticeChangedAsync(ActionConst.Add, itemKey);
                                        break;
                                    case ActionConst.Update:
                                        if (action.OldItem != null)
                                        {
                                            dict.TryRemove(GenerateKey(action.OldItem), out string oldV);
                                        }
                                        dict.AddOrUpdate(itemKey, action.Item.value, (k, v) => { return action.Item.value; });
                                        NoticeChangedAsync(ActionConst.Update, itemKey);
                                        break;
                                    case ActionConst.Remove:
                                        dict.TryRemove(itemKey, out string oldV1);
                                        NoticeChangedAsync(ActionConst.Remove, itemKey);
                                        break;
                                    case ActionConst.Offline:
                                        _adminSayOffline = true;
                                        await WebsocketClient.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None).ConfigureAwait(false);
                                        Logger?.LogTrace("Websocket client offline because admin console send a commond 'offline' ,");
                                        NoticeChangedAsync(ActionConst.Offline);
                                        break;
                                    case ActionConst.Reload:
                                        if (Load())
                                        {
                                            NoticeChangedAsync(ActionConst.Reload);
                                        };
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
            const int maxTry = 5;
            int tryCount = 0;
            while (tryCount < maxTry)
            {
                tryCount++;

                try
                {
                    var op = new AgileHttp.RequestOptions()
                    {
                        Headers = new Dictionary<string, string>()
                        {
                            {"appid", AppId },
                            {"Authorization", GenerateBasicAuthorization(AppId, Secret) }
                        }
                    };
                    var apiUrl = $"{PickOneServer()}/api/config/app/{AppId}";
                    using (var result = AgileHttp.HTTP.Send(apiUrl, "GET", null, op))
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var respContent = result.GetResponseContent();
                            ReloadDataDict(respContent);
                            WriteConfigsToLocal(respContent);

                            Logger?.LogTrace("AgileConfig Client Loaded all the configs success from {0} , Try count: {1}.", apiUrl, tryCount);
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
                    if (tryCount == maxTry)
                    {
                        //load configs from local file
                        var fileContent = ReadConfigsFromLocal();
                        if (!string.IsNullOrEmpty(fileContent))
                        {
                            ReloadDataDict(fileContent);

                            Logger?.LogTrace("AgileConfig Client load all configs from local file .");
                            return true;
                        }
                    }

                    Logger?.LogError(ex, "AgileConfig Client try to load all configs failed . TryCount: {0}", tryCount);
                }
            }

            return false;
        }

        private void ReloadDataDict(string content)
        {
            Data.Clear();
            var concurrentDict = Data as ConcurrentDictionary<string, string>;
            var configs = JsonConvert.DeserializeObject<List<ConfigItem>>(content);
            configs.ForEach(c =>
            {
                var key = GenerateKey(c);
                string value = c.value;
                concurrentDict.TryAdd(key.ToString(), value);
            });
        }

        private const string LocalCacheFileName = "agileconfig.client.configs.cache";
        private void WriteConfigsToLocal(string configContent)
        {
            try
            {
                if (string.IsNullOrEmpty(configContent))
                {
                    return;
                }

                File.WriteAllText(LocalCacheFileName, configContent);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "AgileConfig Client try to cache all configs to local but fail .");
            }
        }

        private string ReadConfigsFromLocal()
        {
            if (!File.Exists(LocalCacheFileName))
            {
                return "";
            }

            return File.ReadAllText(LocalCacheFileName);
        }

        private string DataMd5Version()
        {
            var keyStr = string.Join("&", Data.Keys.ToArray().OrderBy(k => k));
            var valueStr = string.Join("&", Data.Values.ToArray().OrderBy(v => v));
            var txt = $"{keyStr}&{valueStr}";

            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(txt);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

    }
}
