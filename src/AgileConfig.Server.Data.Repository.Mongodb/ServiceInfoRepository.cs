namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ServiceInfoRepository : MongodbRepository<ServiceInfo, string>, IServiceInfoRepository
{
    public ServiceInfoRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ServiceInfoRepository(IConfiguration configuration) : base(configuration)
    {
    }
}