namespace AgileConfig.Server.Data.Repository.Mongodb;

public class RoleFunctionRepository : MongodbRepository<RoleFunction, string>, IRoleFunctionRepository
{
    public RoleFunctionRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public RoleFunctionRepository(IConfiguration configuration) : base(configuration)
    {
    }
}