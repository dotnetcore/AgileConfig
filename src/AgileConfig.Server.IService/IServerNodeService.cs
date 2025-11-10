using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.IService;

public interface IServerNodeService : IDisposable
{
    Task<ServerNode> GetAsync(string id);
    Task<bool> AddAsync(ServerNode node);

    Task<bool> JoinAsync(string ip, int port, string desc);

    Task<bool> DeleteAsync(ServerNode node);

    Task<bool> DeleteAsync(string nodeId);

    Task<bool> UpdateAsync(ServerNode node);

    Task<List<ServerNode>> GetAllNodesAsync();

    /// <summary>
    ///     Initialize server nodes from the nodes section in appsettings.
    /// </summary>
    /// <returns></returns>
    [Obsolete]
    Task<bool> InitWatchNodeAsync();
}