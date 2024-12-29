using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface IServerNodeRepository : IRepository<ServerNode, string>
    {
    }
}
