using System;

namespace Agile.Config.Protocol
{
    public class ActionConst
    {
        public const string Offline = "offline";
        public const string Reload = "reload";
        public const string Ping = "ping";
    }

    public class WebsocketAction
    { 
        public string Model { get; set; }
        public string Action { get; set; }

        public string Data { get; set; }
    }
}
