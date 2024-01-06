using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class SysInitRepository : ISysInitRepository
{
    private readonly IFreeSqlFactory freeSqlFactory;

    public SysInitRepository(IFreeSqlFactory freeSqlFactory)
    {
        this.freeSqlFactory = freeSqlFactory;
    }

    public string? GetJwtTokenSecret()
    {
        var setting = freeSqlFactory.Create().Select<Setting>().Where(x => x.Id == SystemSettings.DefaultJwtSecretKey)
            .ToOne();
        return setting?.Value;
    }

    public void SaveInitSetting(Setting setting)
    {
        freeSqlFactory.Create().Insert(setting).ExecuteAffrows();
    }
}