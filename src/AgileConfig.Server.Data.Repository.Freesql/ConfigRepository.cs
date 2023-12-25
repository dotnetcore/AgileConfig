using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ConfigRepository : FreesqlRepository<Config, string>, IConfigRepository
    {
        public ConfigRepository([FromKeyedServices("Env")]IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory)
        {
        }
    }
}
