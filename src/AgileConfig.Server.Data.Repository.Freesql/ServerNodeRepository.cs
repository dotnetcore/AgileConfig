using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ServerNodeRepository : FreesqlRepository<ServerNode, string>, IServerNodeRepository
    {
        public ServerNodeRepository([FromKeyedServices("")] IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
