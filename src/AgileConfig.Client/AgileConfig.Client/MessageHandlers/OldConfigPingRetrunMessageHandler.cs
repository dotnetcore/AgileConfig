using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client.MessageHandlers
{
    /// <summary>
    /// 老版本的服务端回复配置client的心跳消息的处理类
    /// </summary>
    class OldConfigPingRetrunMessageHandler
    {
        public static bool Hit(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            return message.StartsWith("V:");
        }

        public static async Task Handle(string message, ConfigClient client)
        {
            var version = message.Substring(2, message.Length - 2);
            var localVersion = client.DataMd5Version();
            if (version != localVersion)
            {
                //如果数据库版本跟本地版本不一致则直接全部更新
                await client.Load();
            }
        }
    }
}
