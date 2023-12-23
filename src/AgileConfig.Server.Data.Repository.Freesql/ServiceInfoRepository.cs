using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ServiceInfoRepository : FreesqlRepository<ServiceInfo, string>, IServiceInfoRepository
    {
        public ServiceInfoRepository(IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
