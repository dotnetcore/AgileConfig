using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Data.Repository.Mongodb
{
    public class RoleDefinitionRepository : MongodbRepository<RoleDefinition, string>, IRoleDefinitionRepository
    {
        public RoleDefinitionRepository(string? connectionString) : base(connectionString)
        {
        }

        public RoleDefinitionRepository(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
