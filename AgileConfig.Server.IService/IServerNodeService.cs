using AgileConfig.Server.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IServerNodeService
    {
        Task<ServerNode> GetAsync(string id);
        Task<bool> AddAsync(ServerNode node);

        Task<bool> DeleteAsync(ServerNode node);

        Task<bool> DeleteAsync(string nodeId);

        Task<bool> UpdateAsync(ServerNode node);

        Task<List<ServerNode>> GetAllNodesAsync();
    }
}
