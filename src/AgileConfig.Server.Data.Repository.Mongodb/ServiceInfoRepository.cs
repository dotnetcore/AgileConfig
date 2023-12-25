namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ServiceInfoRepository: MongodbRepository<ServiceInfo, string>, IServiceInfoRepository
{
    public ServiceInfoRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ServiceInfoRepository(IConfiguration configuration) : base(configuration)
    {
    }
}