using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client.MessageHandlers
{
    /// <summary>
    /// 老版本的服务端回复配置client的心跳消息的处理类
    /// </summary>
    class DropMessageHandler
    {
        public static bool Hit(string message)
        {
            return string.IsNullOrEmpty(message) || message == "0";
        }
    }
}
