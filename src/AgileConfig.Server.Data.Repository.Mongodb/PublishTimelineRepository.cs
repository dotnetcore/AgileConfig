namespace AgileConfig.Server.Data.Repository.Mongodb;

public class PublishTimelineRepository: MongodbRepository<PublishTimeline, string>, IPublishTimelineRepository
{
    public PublishTimelineRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public PublishTimelineRepository(IConfiguration configuration) : base(configuration)
    {
    }
}