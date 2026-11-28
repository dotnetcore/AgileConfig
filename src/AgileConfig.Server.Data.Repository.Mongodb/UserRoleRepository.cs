namespace AgileConfig.Server.Data.Repository.Mongodb;

public class UserRoleRepository : MongodbRepository<UserRole, string>, IUserRoleRepository
{
    public UserRoleRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public UserRoleRepository(IConfiguration configuration) : base(configuration)
    {
    }
}