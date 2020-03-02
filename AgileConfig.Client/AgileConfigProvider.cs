using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace AgileConfig.Client
{


    public class AgileConfigProvider : ConfigurationProvider
    {
        protected ILogger Logger { get; }

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
            Logger = loggerFactory?.CreateLogger<AgileConfigProvider>();
            ConfigClient.Instance.Logger = loggerFactory?.CreateLogger<ConfigClient>();
            ConfigClient.Instance.ServerNodeHost = host;
            ConfigClient.Instance.AppId = appId;
            ConfigClient.Instance.Secret = secret;
        }

        /// <summary>
        /// load方法会通过http从服务端拉取所有的配置，需要注意的是load方法在加载所有配置后会启动一个websocket客户端跟服务端保持长连接，当websocket
        /// 连接建立成功会调用一次load方法，所以系统刚启动的时候通常会出现两次http请求。
        /// </summary>
        public override void Load()
        {
            if (ConfigClient.Instance.LoadAllConfigNodes())
            {
                Data = ConfigClient.Instance.Data;
                ConfigClient.Instance.WebsocketConnect();
                ConfigClient.Instance.WebsocketHeartbeatAsync();
            }
            else
            {
                throw new Exception("AgileConfig client can not load configs from server .");
            }
        }
    }
}
