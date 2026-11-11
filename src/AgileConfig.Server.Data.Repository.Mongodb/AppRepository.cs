namespace AgileConfig.Server.Data.Repository.Mongodb;

public class AppRepository : MongodbRepository<App, string>, IAppRepository
{
    public AppRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public AppRepository(IConfiguration configuration) : base(configuration)
    {
    }
}