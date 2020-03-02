using System;

namespace Agile.Config.Protocol
{
    public class ConfigItem
    {
        public string key { get; set; }

        public string value { get; set; }

        public string group { get; set; }
    }

    public class WebsocketAction
    {
        public string Action { get; set; }

        public ConfigItem Item { get; set; }

        public ConfigItem OldItem { get; set; }
    }
}
