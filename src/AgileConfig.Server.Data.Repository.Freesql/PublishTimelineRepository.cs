using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class PublishTimelineRepository : FreesqlRepository<PublishTimeline, string>, IPublishTimelineRepository
    {
        public PublishTimelineRepository(IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
