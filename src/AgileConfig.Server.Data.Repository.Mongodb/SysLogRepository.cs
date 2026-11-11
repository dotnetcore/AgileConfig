namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SysLogRepository : MongodbRepository<SysLog, string>, ISysLogRepository
{
    public SysLogRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public SysLogRepository(IConfiguration configuration) : base(configuration)
    {
    }
}