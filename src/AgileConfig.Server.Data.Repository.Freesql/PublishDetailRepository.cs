using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class PublishDetailRepository : FreesqlRepository<PublishDetail, string>, IPublishDetailRepository
    {
        public PublishDetailRepository([FromKeyedServices("Env")] IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
