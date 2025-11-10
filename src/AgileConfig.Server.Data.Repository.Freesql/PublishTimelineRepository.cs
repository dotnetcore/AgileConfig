using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class PublishTimelineRepository : FreesqlRepository<PublishTimeline, string>, IPublishTimelineRepository
{
    private readonly IFreeSql freeSql;

    public PublishTimelineRepository(IFreeSql freeSql) : base(freeSql)
    {
        this.freeSql = freeSql;
    }

    public async Task<string> GetLastPublishTimelineNodeIdAsync(string appId, string env)
    {
        var node = await freeSql.Select<PublishTimeline>()
            .Where(x => x.AppId == appId && x.Env == env)
            .OrderByDescending(x => x.Version)
            .FirstAsync();

        return node?.Id;
    }
}