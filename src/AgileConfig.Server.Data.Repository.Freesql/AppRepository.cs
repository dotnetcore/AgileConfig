using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class AppRepository : FreesqlRepository<App, string>, IAppRepository
    {
        public AppRepository(IFreeSql freeSql) : base(freeSql)
        {
        }
    }
}
