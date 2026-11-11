namespace AgileConfig.Server.Data.Repository.Mongodb;

public class PublishDetailRepository : MongodbRepository<PublishDetail, string>, IPublishDetailRepository
{
    public PublishDetailRepository(string? connectionString, MongodbTlsOptions tlsOptions = null)
        : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public PublishDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }
}