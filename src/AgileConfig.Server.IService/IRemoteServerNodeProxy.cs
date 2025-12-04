using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agile.Config.Protocol;

namespace AgileConfig.Server.IService;

public class ClientInfo
{
    public string Id { get; set; }

    public string AppId { get; set; }

    public string Address { get; set; }

    public string Tag { get; set; }

    public string Name { get; set; }

    public string Ip { get; set; }

    public string Env { get; set; }

    public DateTime LastHeartbeatTime { get; set; }

    public DateTime? LastRefreshTime { get; set; }
}

public class ClientInfos
{
    public int ClientCount { get; set; }

    public List<ClientInfo> Infos { get; set; }
}

public interface IRemoteServerNodeProxy
{
    /// <summary>
    ///     Notify all clients on a node to execute an action.
    /// </summary>
    /// <param name="address">Server address that hosts the clients.</param>
    /// <param name="action">Action to be executed on each client.</param>
    /// <returns>True when the command is sent successfully.</returns>
    Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action);

    /// <summary>
    ///     Notify a specific client on a node to execute an action.
    /// </summary>
    /// <param name="address">Server address that hosts the client.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="clientId">Client identifier that should receive the action.</param>
    /// <returns>True when the command is sent successfully.</returns>
    Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action);

    /// <summary>
    ///     Notify clients of an application on a node to execute an action.
    /// </summary>
    /// <param name="address">Server address that hosts the clients.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="appId">Application ID whose clients should receive the action.</param>
    /// <param name="env">Environment of the target clients.</param>
    /// <returns>True when the command is sent successfully.</returns>
    Task<bool> AppClientsDoActionAsync(string address, string appId, string env, WebsocketAction action);

    /// <summary>
    ///     Retrieve the client connection report for a node.
    /// </summary>
    /// <param name="address">Server address to query.</param>
    /// <returns>Collection of connected clients.</returns>
    Task<ClientInfos> GetClientsReportAsync(string address);

    /// <summary>
    ///     Check whether all nodes are online.
    /// </summary>
    /// <returns></returns>
    Task TestEchoAsync();

    /// <summary>
    ///     Check whether a specific node is online.
    /// </summary>
    /// <param name="address">Server address to ping.</param>
    /// <returns>Task that completes when the echo test finishes.</returns>
    Task TestEchoAsync(string address);

    /// <summary>
    ///     Clear the configuration cache on a node.
    /// </summary>
    /// <param name="address">Server address whose cache should be cleared.</param>
    /// <returns>Task that completes when the cache is cleared.</returns>
    Task ClearConfigServiceCache(string address);

    /// <summary>
    ///     Clear the service registration cache on a node.
    /// </summary>
    /// <param name="address">Server address whose cache should be cleared.</param>
    /// <returns>Task that completes when the cache is cleared.</returns>
    Task ClearServiceInfoCache(string address);
}