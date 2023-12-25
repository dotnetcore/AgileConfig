namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SysLogRepository: MongodbRepository<SysLog, int>, ISysLogRepository
{
    public SysLogRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public SysLogRepository(IConfiguration configuration) : base(configuration)
    {
    }
}