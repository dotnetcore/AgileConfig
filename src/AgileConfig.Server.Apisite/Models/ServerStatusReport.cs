using AgileConfig.Server.IService;
using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models
{
    [ExcludeFromCodeCoverage]
    public class ServerStatusReport
    {
        public ClientInfos WebsocketCollectionReport { get; set; }

        public int AppCount { get; set; }

        public int ConfigCount { get; set; }

        public int NodeCount { get; set; }
    }
}
