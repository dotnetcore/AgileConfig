namespace AgileConfig.Server.Data.Repository.Mongodb;

public class UserAppAuthRepository: MongodbRepository<UserAppAuth, string>, IUserAppAuthRepository
{
    public UserAppAuthRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public UserAppAuthRepository(IConfiguration configuration) : base(configuration)
    {
    }
}