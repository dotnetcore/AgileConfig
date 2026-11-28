namespace AgileConfig.Server.Data.Repository.Mongodb;

public class RoleFunctionRepository : MongodbRepository<RoleFunction, string>, IRoleFunctionRepository
{
    public RoleFunctionRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public RoleFunctionRepository(IConfiguration configuration) : base(configuration)
    {
    }
}