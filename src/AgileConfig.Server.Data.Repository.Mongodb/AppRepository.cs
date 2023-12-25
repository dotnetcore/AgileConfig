namespace AgileConfig.Server.Data.Repository.Mongodb;

public class AppRepository: MongodbRepository<App, string>, IAppRepository
{
    public AppRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public AppRepository(IConfiguration configuration) : base(configuration)
    {
    }
}