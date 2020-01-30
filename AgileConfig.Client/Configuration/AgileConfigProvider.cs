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

        private ClientWebSocket WebsocketClient { get; set; }

        private string WSUrl { get; }

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
            WSUrl += "/ws?appid=" + appId;
        }

        /// <summary>
        /// 开启一个线程来初始化Websocket Client，并且5s一次进行检查是否连接打开状态，如果不是则尝试重连。
        /// </summary>
        /// <returns></returns>
        private Task WebsocketConnect()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000 * 5);
                    if (WebsocketClient?.State == WebSocketState.Open)
                    {
                        continue;
                    }
                    try
                    {
                        WebsocketClient?.Abort();
                        WebsocketClient?.Dispose();
                        WebsocketClient = null;
                        WebsocketClient = new ClientWebSocket();
                        await WebsocketClient.ConnectAsync(new Uri(WSUrl), CancellationToken.None);
                        Logger?.LogTrace("AgileConfig Client Websocket Connected , {0}", WSUrl);
                        HandleWebsocketMessageAsync();
                        //连接成功重新加载配置
                        LoadAll();
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "AgileConfig Client Websocket try to connected to server failed.");
                    }
                }
            });
        }
        /// <summary>
        /// 开启一个线程5s进行一次心跳
        /// </summary>
        /// <returns></returns>
        private void WebsocketHeartbeatAsync()
        {
            Task.Run(async () =>
            {
                var data = Encoding.UTF8.GetBytes("hi");
                while (true)
                {
                    await Task.Delay(1000 * 5);
                    if (WebsocketClient?.State == WebSocketState.Open)
                    {
                        try
                        {
                            //这里由于多线程的问题，WebsocketClient有可能在上一个if判断成功后被置空或者断开，所以需要try一下避免线程退出
                            await WebsocketClient.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true,
                                    CancellationToken.None);
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
                        result = await WebsocketClient.ReceiveAsync(buffer, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "AgileConfig Client Websocket try to ReceiveAsync message occur exception .");
                        throw;
                    }
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
                                            ProcessAction(action);
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

        /// <summary>
        /// 最终处理服务端推送的动作
        /// </summary>
        /// <param name="action"></param>
        private void ProcessAction(WebsocketAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
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

        /// <summary>
        /// load方法会通过http从服务端拉取所有的配置，需要注意的是load方法在加载所有配置后会启动一个websocket客户端跟服务端保持长连接，当websocket
        /// 连接建立成功会调用一次load方法，所以系统刚启动的时候通常会出现两次http请求。
        /// </summary>
        public override void Load()
        {
            if (LoadAll())
            {
                WebsocketConnect();
                WebsocketHeartbeatAsync();
            }
            else
            {
                throw new Exception("AgileConfig client can not load configs from server .");
            }
        }

        /// <summary>
        /// 通过http从server拉取所有配置到本地，尝试5次
        /// </summary>
        private bool LoadAll()
        {
            var apiUrl = $"{Host}/api/config/app/{AppId}";
            int tryCount = 0;
            while (tryCount <= 4)
            {
                tryCount++;

                try
                {
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
                            Logger?.LogTrace("AgileConfig Client Loaded all the configs success from {0} .", apiUrl);
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
                    Logger?.LogError(ex, "AgileConfig Client try to load all configs failed . TryCount: {0}", tryCount);
                }
            }

            return false;
        }
    }
}
