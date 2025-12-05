namespace AgileConfig.Server.Data.Repository.Mongodb;

public class UserRoleRepository : MongodbRepository<UserRole, string>, IUserRoleRepository
{
    public UserRoleRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public UserRoleRepository(IConfiguration configuration) : base(configuration)
    {
    }
}