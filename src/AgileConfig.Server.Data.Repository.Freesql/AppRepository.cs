using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class AppRepository : FreesqlRepository<App, string>, IAppRepository
    {
        private readonly IFreeSqlFactory freeSqlFactory;

        public AppRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
        {
            this.freeSqlFactory = freeSqlFactory;
        }
    }
}
