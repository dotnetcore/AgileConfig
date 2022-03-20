using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Apisite.Websocket.MessageHandlers;

public interface IMessageHandler
{
    bool Hit(HttpRequest request);
    Task Handle(string message, HttpRequest request, WebSocket client);
}