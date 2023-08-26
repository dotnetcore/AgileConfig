using AgileConfig.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client.MessageHandlers
{
    /// <summary>
    /// 服务端回复配置client的action消息的处理类
    /// </summary>
    class ConfigCenterActionMessageHandler
    {
        public static bool Hit(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            try
            {
                var action = JsonConvert.DeserializeObject<ActionMessage>(message);
                if (action == null)
                {
                    return false;
                }

                return action.Module == ActionModule.ConfigCenter;
            }
            catch 
            {
            }

            return false;
        }

        public static async Task Handle(string message, ConfigClient client)
        {
            var action = JsonConvert.DeserializeObject<ActionMessage>(message);
            if (action != null)
            {
                await client.TryHandleAction(action);
            }
        }
    }
}
