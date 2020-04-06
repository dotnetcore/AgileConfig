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
        public const string Add = "add";
        public const string Update = "update";
        public const string Remove = "remove";
        public const string Offline = "offline";
        public const string Reload = "reload";
    }

    public class WebsocketAction
    {
        public string Action { get; set; }

        public ConfigItem Item { get; set; }

        public ConfigItem OldItem { get; set; }
    }
}
