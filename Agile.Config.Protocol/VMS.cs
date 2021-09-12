using System;

namespace Agile.Config.Protocol
{
    public class ConfigItem
    {
        public string key { get; set; }

        public string value { get; set; }

        public string group { get; set; }
    }

    public class ActionConst
    {
        public const string Offline = "offline";
        public const string Reload = "reload";
    }

    public class WebsocketAction
    {
        public string Action { get; set; }

    }
}
