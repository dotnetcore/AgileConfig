using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Agile.Config.Client
{
    public interface IConfigClient
    {
        string this[string key] { get; }

        ConcurrentDictionary<string, string> Data { get; }

        void Connect();
    }
}