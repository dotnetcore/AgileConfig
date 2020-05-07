using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public class ClientInfo
    {
        public string Id { get; set; }

        public string AppId { get; set; }

        public DateTime LastHeartbeatTime { get; set; }
    }

    public class ClientInfos
    {
        public int ClientCount { get; set; }

        public List<ClientInfo> Infos { get; set; }
    }

    public interface IRemoteServerNodeManager
    {
        Task TestEchoAsync();

        ClientInfos GetClientsReport(string address);

        IRemoteServerNodeActionProxy NodeProxy { get; }
    }
}
