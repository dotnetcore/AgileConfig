using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction;

public interface IRoleDefinitionRepository : IRepository<Role, string>
{
}