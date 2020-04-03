using System.Collections.Concurrent;

namespace Agile.Config.Client
{
    public interface IConfigClient
    {
        string this[string key] { get; }

        ConcurrentDictionary<string, string> Data { get; }
    }
}