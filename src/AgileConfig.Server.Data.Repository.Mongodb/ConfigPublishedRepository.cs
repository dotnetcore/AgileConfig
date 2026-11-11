namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ConfigPublishedRepository : MongodbRepository<ConfigPublished, string>, IConfigPublishedRepository
{
    public ConfigPublishedRepository(string? connectionString, MongodbTlsOptions tlsOptions = null)
        : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ConfigPublishedRepository(IConfiguration configuration) : base(configuration)
    {
    }
}