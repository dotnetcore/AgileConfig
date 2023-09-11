using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace AgileConfig.Client
{
    public class AgileConfigProvider : ConfigurationProvider
    {
        private ConfigClient Client { get; }

        public AgileConfigProvider(IConfigClient client)
        {
            Client = client as ConfigClient;
            Client.ConfigChanged += (arg) =>
            {
                this.OnReload();
            };
        }

        /// <summary>
        /// load方法调用ConfigClient的Connect方法,Connect方法会在连接成功后拉取所有的配置。
        /// </summary>
        public override void Load()
        {
            Client.ConnectAsync().GetAwaiter().GetResult() ;
            Data = Client.Data;
        }

    }
}
