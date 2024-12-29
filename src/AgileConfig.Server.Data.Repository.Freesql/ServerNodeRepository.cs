using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ServerNodeRepository : FreesqlRepository<ServerNode, string>, IServerNodeRepository
    {
        private readonly IFreeSqlFactory freeSqlFactory;

        public ServerNodeRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
        {
            this.freeSqlFactory = freeSqlFactory;
        }
    }
}
