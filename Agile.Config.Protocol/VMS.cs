using System;

namespace Agile.Config.Protocol
{
    public class ActionConst
    {
        public const string Offline = "offline";
        public const string Reload = "reload";
        public const string Ping = "ping";
    }

    public class ActionModule
    {
        public const string RegisterCenter = "r";
        public const string ConfigCenter = "c";
    }

    public class WebsocketAction
    {
        public WebsocketAction()
        {
        }
        public string Module { get; set; }
        public string Action { get; set; }

        public string Data { get; set; }
    }
}
