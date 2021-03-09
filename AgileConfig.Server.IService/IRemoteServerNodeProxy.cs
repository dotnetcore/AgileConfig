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

        public string Address { get; set; }

        public string Tag { get;set; }

        public DateTime LastHeartbeatTime { get; set; }
    }

    public class ClientInfos
    {
        public int ClientCount { get; set; }

        public List<ClientInfo> Infos { get; set; }
    }

    public interface IRemoteServerNodeProxy
    {
        /// <summary>
        /// 通知某个节点所有的客户端执行某个动作
        /// </summary>
        /// <param name="address"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action);

        /// <summary>
        /// 通知某个节点某个客户端执行某个动作
        /// </summary>
        /// <param name="address"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action);

        /// <summary>
        /// 通知某个节点某个相关app的客户端执行某个动作
        /// </summary>
        /// <param name="address"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<bool> AppClientsDoActionAsync(string address, string appId, WebsocketAction action);

        /// <summary>
        /// 获取某个节点的客户端连接信息
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        ClientInfos GetClientsReport(string address);

        /// <summary>
        /// 探测所有个节点是否在线
        /// </summary>
        /// <returns></returns>
        Task TestEchoAsync();

        /// <summary>
        /// 探测某个节点是否在校
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task TestEchoAsync(string address);
    }
}
