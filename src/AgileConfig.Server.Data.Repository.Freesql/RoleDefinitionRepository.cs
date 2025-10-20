using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class RoleDefinitionRepository : FreesqlRepository<RoleDefinition, string>, IRoleDefinitionRepository
    {
        public RoleDefinitionRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
        {
        }
    }
}
