using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class SysInitRepository : ISysInitRepository
{
    public string? GetJwtTokenSecret()
    {
        var setting = FreeSQL.Instance.Select<Setting>().Where(x => x.Id == SystemSettings.DefaultJwtSecretKey)
            .ToOne();
        return setting?.Value;
    }

    public void SaveInitSetting(Setting setting)
    {
        FreeSQL.Instance.Insert(setting);
    }
}