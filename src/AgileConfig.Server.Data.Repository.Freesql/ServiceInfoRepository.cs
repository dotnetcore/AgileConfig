using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ServiceInfoRepository : FreesqlRepository<ServiceInfo, string>, IServiceInfoRepository
    {
        private readonly IFreeSql freeSql;

        public ServiceInfoRepository(IFreeSql freeSql) : base(freeSql)
        {
            this.freeSql = freeSql;
        }
    }
}
