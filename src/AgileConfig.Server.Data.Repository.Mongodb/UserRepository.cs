namespace AgileConfig.Server.Data.Repository.Mongodb;

public class UserRepository : MongodbRepository<User, string>, IUserRepository
{
    public UserRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public UserRepository(IConfiguration configuration) : base(configuration)
    {
    }
}