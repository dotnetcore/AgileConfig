using AgileConfig.Server.Common;
using MongoDB.Driver;

namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SysInitRepository : ISysInitRepository
{
    public SysInitRepository(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    private MongodbAccess<Setting> _access => new MongodbAccess<Setting>(_configuration["db:conn"]);
    private readonly IConfiguration _configuration;

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