namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ConfigRepository : MongodbRepository<Config, string>, IConfigRepository
{
    public ConfigRepository(string? connectionString, MongodbTlsOptions tlsOptions = null)
        : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ConfigRepository(IConfiguration configuration) : base(configuration)
    {
    }
}