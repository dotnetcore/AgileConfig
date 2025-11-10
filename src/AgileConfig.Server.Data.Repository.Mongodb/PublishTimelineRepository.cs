namespace AgileConfig.Server.Data.Repository.Mongodb;

public class PublishTimelineRepository : MongodbRepository<PublishTimeline, string>, IPublishTimelineRepository
{
    public PublishTimelineRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public PublishTimelineRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<string> GetLastPublishTimelineNodeIdAsync(string appId, string env)
    {
        var nodes = await QueryPageAsync(x => x.AppId == appId && x.Env == env, 1, 1, nameof(PublishTimeline.Version),
            "DESC");

        return nodes?.FirstOrDefault()?.Id;
    }
}