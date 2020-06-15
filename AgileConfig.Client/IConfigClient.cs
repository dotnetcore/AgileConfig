using Agile.Config.Protocol;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Agile.Config.Client
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
        string this[string key] { get; }

        ConcurrentDictionary<string, string> Data { get; }

        Task<bool> ConnectAsync();

        bool Load();

        event Action<ConfigChangedArg> ConfigChanged;
    }
}