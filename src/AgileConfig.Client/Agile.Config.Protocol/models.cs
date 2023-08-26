using System;

namespace AgileConfig.Protocol
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

        public const string Ping = "ping";

    }

    public class ActionMessage
    {
        public string Module { get; set; }
        public string Action { get; set; }

        public string Data { get; set; }
    }

    public class ActionModule
    {
        public const string RegisterCenter = "r";
        public const string ConfigCenter = "c";
    }
}
