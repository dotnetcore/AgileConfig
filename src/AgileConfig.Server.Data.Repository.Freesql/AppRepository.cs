using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class AppRepository : FreesqlRepository<App, string>, IAppRepository
    {
        private readonly IFreeSql freeSql;

        public AppRepository(IFreeSql freeSql) : base(freeSql)
        {
            this.freeSql = freeSql;
        }
    }
}
