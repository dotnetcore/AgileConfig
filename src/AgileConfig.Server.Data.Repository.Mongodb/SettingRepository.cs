namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SettingRepository : MongodbRepository<Setting, string>, ISettingRepository
{
    public SettingRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public SettingRepository(IConfiguration configuration) : base(configuration)
    {
    }
}