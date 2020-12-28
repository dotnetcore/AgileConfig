using Agile.Config.Protocol;
using AgileConfig.Server.IService;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Websocket
{
    public class WebsocketClient : ClientInfo
    {
        public WebSocket Client { get; set; }
    }

    public interface IWebsocketCollection
    {
        ClientInfos Report();

        WebsocketClient Get(string clientId);
        void SendToAll(string message);
        Task SendMessageToOne(WebsocketClient client, string message);

        Task SendActionToOne(WebsocketClient client, WebsocketAction action);
        void AddClient(WebsocketClient client);

        Task RemoveClient(WebsocketClient client, WebSocketCloseStatus? closeStatus, string closeDesc);

        void RemoveAppClients(string appId, WebSocketCloseStatus? closeStatus, string closeDesc);

        void SendActionToAppClients(string appId, WebsocketAction action);

        void SendActionToAll(WebsocketAction action);

        void SendToAppClients(string appId, string message);
    }

}
