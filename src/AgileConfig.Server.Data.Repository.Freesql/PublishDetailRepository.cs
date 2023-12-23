using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class PublishDetailRepository : FreesqlRepository<PublishDetail, string>, IPublishDetailRepository
    {
        public PublishDetailRepository(IFreeSql freeSql) : base(freeSql)
        {
        }
    }
}
