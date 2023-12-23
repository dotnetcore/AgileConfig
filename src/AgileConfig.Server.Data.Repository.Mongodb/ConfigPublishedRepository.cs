namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ConfigPublishedRepository : MongodbRepository<ConfigPublished, string>, IConfigPublishedRepository
{
    public ConfigPublishedRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ConfigPublishedRepository(IConfiguration configuration) : base(configuration)
    {
    }
}