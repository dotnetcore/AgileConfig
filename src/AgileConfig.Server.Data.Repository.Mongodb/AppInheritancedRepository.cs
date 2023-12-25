namespace AgileConfig.Server.Data.Repository.Mongodb;

public class AppInheritancedRepository : MongodbRepository<AppInheritanced, string>, IAppInheritancedRepository
{
    public AppInheritancedRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public AppInheritancedRepository(IConfiguration configuration) : base(configuration)
    {
    }
}