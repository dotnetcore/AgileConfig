using Agile.Config.Protocol;
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

    public interface IRemoteServerNodeProxy
    {
        Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action);

        Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action);

        Task<bool> AppClientsDoActionAsync(string address, string appId, WebsocketAction action);

        ClientInfos GetClientsReport(string address);
        Task TestEchoAsync();

    }
}
