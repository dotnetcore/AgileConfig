using Agile.Config.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IRemoteServerNodeProxy
    {
        Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action);

        Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action);

        Task<bool> AppClientsDoActionAsync(string address, string appId, WebsocketAction action);

    }
}
