using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface IConfigRepository : IRepository<Config, string>
    {
    }
}
