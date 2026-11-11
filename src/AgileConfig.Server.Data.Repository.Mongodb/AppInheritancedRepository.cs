namespace AgileConfig.Server.Data.Repository.Mongodb;

public class AppInheritancedRepository : MongodbRepository<AppInheritanced, string>, IAppInheritancedRepository
{
    public AppInheritancedRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public AppInheritancedRepository(IConfiguration configuration) : base(configuration)
    {
    }
}