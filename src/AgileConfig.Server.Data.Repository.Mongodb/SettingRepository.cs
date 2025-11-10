namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SettingRepository : MongodbRepository<Setting, string>, ISettingRepository
{
    public SettingRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public SettingRepository(IConfiguration configuration) : base(configuration)
    {
    }
}