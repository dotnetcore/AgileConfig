using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ConfigPublishedRepository : FreesqlRepository<ConfigPublished, string>, IConfigPublishedRepository
    {
        public ConfigPublishedRepository([FromKeyedServices("Env")] IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
