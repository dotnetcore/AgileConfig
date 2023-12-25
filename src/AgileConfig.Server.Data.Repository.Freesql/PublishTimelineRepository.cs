using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class PublishTimelineRepository : FreesqlRepository<PublishTimeline, string>, IPublishTimelineRepository
    {
        public PublishTimelineRepository([FromKeyedServices("Env")] IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
