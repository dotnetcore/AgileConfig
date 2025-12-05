using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction;

public interface IAppRepository : IRepository<App, string>
{
}