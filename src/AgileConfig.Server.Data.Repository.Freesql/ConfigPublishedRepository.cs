using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class ConfigPublishedRepository : FreesqlRepository<ConfigPublished, string>, IConfigPublishedRepository
{
    private readonly IFreeSql freeSql;

    public ConfigPublishedRepository(IFreeSql freeSql) : base(freeSql)
    {
        this.freeSql = freeSql;
    }
}