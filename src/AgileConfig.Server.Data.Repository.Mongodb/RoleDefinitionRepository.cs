namespace AgileConfig.Server.Data.Repository.Mongodb;

public class RoleDefinitionRepository : MongodbRepository<Role, string>, IRoleDefinitionRepository
{
    public RoleDefinitionRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    public RoleDefinitionRepository(IConfiguration configuration) : base(configuration)
    {
    }
}