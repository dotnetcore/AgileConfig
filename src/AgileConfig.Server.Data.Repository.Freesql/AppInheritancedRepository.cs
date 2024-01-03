using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class AppInheritancedRepository : FreesqlRepository<AppInheritanced, string>, IAppInheritancedRepository
    {
        private readonly IFreeSql freeSql;

        public AppInheritancedRepository(IFreeSql freeSql) : base(freeSql)
        {
            this.freeSql = freeSql;
        }
    }
}
