using AgileConfig.Server.Common;
using MongoDB.Driver;

namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SysInitRepository : ISysInitRepository
{
    private readonly MongodbAccess<Setting> _access = new(Global.Config["db:conn"]);

    public  Task<string?> GetDefaultEnvironmentAsync()
    {
        var setting = _access.MongoQueryable.FirstOrDefault(x => x.Id == SystemSettings.DefaultEnvironmentKey);
        var val = setting?.Value;

        return Task.FromResult(val);
    }

    public string? GetJwtTokenSecret()
    {
        var setting = _access.MongoQueryable.FirstOrDefault(x => x.Id == SystemSettings.DefaultJwtSecretKey);
        return setting?.Value;
    }

    public void SaveInitSetting(Setting setting)
    {
        _access.Collection.InsertOne(setting);
    }
}