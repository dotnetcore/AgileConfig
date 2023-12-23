using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ConfigRepository : FreesqlRepository<Config, string>, IConfigRepository
    {
        public ConfigRepository(IFreeSql freeSql) : base(freeSql)
        {
        }
    }
}
