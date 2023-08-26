using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using AgileConfig.Protocol;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Client
{
    public class ConfigChangedArg
    {
        public ConfigChangedArg(string action, string key)
        {
            Action = action;
            Key = key;
        }

        public string Key { get; }

        public string Action { get; }
    }

    public interface IConfigClient
    {
        ConnectStatus Status { get; }

        string this[string key] { get; }

        string Get(string key);

        List<ConfigItem> GetGroup(string groupName);

        ConcurrentDictionary<string, string> Data { get; }

        Task<bool> ConnectAsync();

        Task<bool> Load();

        void LoadConfigs(List<ConfigItem> configs);

        event Action<ConfigChangedArg> ConfigChanged;

        ILogger Logger { get; set; }

        ConfigClientOptions Options { get; }

        ClientWebSocket WebSocket { get;  }

    }
}