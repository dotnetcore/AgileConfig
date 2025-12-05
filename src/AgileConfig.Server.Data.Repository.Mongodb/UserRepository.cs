namespace AgileConfig.Server.Data.Repository.Mongodb;

public class UserRepository : MongodbRepository<User, string>, IUserRepository
{
    public UserRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public UserRepository(IConfiguration configuration) : base(configuration)
    {
    }
}