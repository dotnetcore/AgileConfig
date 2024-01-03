using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class PublishTimelineRepository : FreesqlRepository<PublishTimeline, string>, IPublishTimelineRepository
    {
        private readonly IFreeSql freeSql;

        public PublishTimelineRepository(IFreeSql freeSql) : base(freeSql)
        {
            this.freeSql = freeSql;
        }
    }
}
