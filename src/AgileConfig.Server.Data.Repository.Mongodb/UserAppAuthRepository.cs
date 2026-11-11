namespace AgileConfig.Server.Data.Repository.Mongodb;

public class UserAppAuthRepository : MongodbRepository<UserAppAuth, string>, IUserAppAuthRepository
{
    public UserAppAuthRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public UserAppAuthRepository(IConfiguration configuration) : base(configuration)
    {
    }
}