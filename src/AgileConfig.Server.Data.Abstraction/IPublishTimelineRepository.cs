using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface IPublishTimelineRepository : IRepository<PublishTimeline, string>
    {
        Task<string> GetLastPublishTimelineNodeIdAsync(string appId, string env);
    }
}
