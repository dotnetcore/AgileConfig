namespace AgileConfig.Server.Data.Repository.Mongodb;

public class PublishDetailRepository : MongodbRepository<PublishDetail, string>, IPublishDetailRepository
{
    public PublishDetailRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public PublishDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }
}