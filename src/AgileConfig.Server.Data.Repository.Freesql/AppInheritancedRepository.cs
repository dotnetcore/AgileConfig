using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class AppInheritancedRepository : FreesqlRepository<AppInheritanced, string>, IAppInheritancedRepository
    {
        public AppInheritancedRepository(IFreeSql freeSql) : base(freeSql)
        {
        }
    }
}
