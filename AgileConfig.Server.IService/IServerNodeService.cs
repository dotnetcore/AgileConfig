using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IServerNodeService: IDisposable
    {
        Task<ServerNode> GetAsync(string id);
        Task<bool> AddAsync(ServerNode node);

        Task<bool> DeleteAsync(ServerNode node);

        Task<bool> DeleteAsync(string nodeId);

        Task<bool> UpdateAsync(ServerNode node);

        Task<List<ServerNode>> GetAllNodesAsync();
        
        /// <summary>
        /// 根据appsettings里的nodes配置初始化服务器节点
        /// </summary>
        /// <returns></returns>
        Task<bool> InitWatchNodeAsync();
    }
}
