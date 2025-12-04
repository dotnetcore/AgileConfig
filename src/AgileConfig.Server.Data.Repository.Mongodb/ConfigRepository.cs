namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ConfigRepository : MongodbRepository<Config, string>, IConfigRepository
{
    public ConfigRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ConfigRepository(IConfiguration configuration) : base(configuration)
    {
    }
}