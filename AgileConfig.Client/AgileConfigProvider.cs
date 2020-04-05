using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Agile.Config.Client
{
    public class AgileConfigProvider : ConfigurationProvider
    {
        private ILogger Logger { get; }

        private ConfigClient Client { get; }

        public AgileConfigProvider(IConfigClient client, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory?.CreateLogger<AgileConfigProvider>();
            Client = client as ConfigClient;
        }

        /// <summary>
        /// load方法会通过http从服务端拉取所有的配置，需要注意的是load方法在加载所有配置后会启动一个websocket客户端跟服务端保持长连接，当websocket
        /// 连接建立成功会调用一次load方法，所以系统刚启动的时候通常会出现两次http请求。
        /// </summary>
        public override void Load()
        {
            if (Client.Load())
            {
                Data = Client.Data;
                Client.Connect();
            }
            else
            {
                throw new Exception("AgileConfig client can not load configs from server .");
            }
        }
    }
}
