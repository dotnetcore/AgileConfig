using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class ConfigPublishedRepository : FreesqlRepository<ConfigPublished, string>, IConfigPublishedRepository
    {
        public ConfigPublishedRepository(IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
