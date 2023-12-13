using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Mongodb.Service;

public class AppService(
    IRepository<App> repository,
    IRepository<AppInheritanced> appInheritancedRepository,
    IRepository<Config> configRepository,
    IRepository<ConfigPublished> configPublishedRepository,
    IRepository<UserAppAuth> userAppAuthRepository,
    IRepository<User> userRepository):IAppService
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<App?> GetAsync(string id)
    {
        return repository.FindAsync(id);
    }

    public async Task<bool> AddAsync(App app)
    {
        await repository.InsertAsync(app);
        return true;
    }

    public async Task<bool> AddAsync(App app, List<AppInheritanced>? appInheritanceds)
    {
        await repository.InsertAsync(app);
        if (appInheritanceds != null)
        {
            await appInheritancedRepository.InsertAsync(appInheritanceds);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(App app)
    {
        var app2 = await repository.FindAsync(app.Id);
        if (app2 != null)
        {
            await repository.DeleteAsync(app2.Id);
            //怕有的同学误删app导致要恢复，所以保留配置项吧。
            var update = Builders<Config>.Update.Set(x => x.Status, ConfigStatus.Deleted);
            await configRepository.UpdateAsync(x => x.AppId == app2.Id, update);
            //删除发布的配置项
            var publishedConfigUpdate = Builders<ConfigPublished>.Update.Set(x => x.Status, ConfigStatus.Deleted);
            await configPublishedRepository.UpdateAsync(x => x.AppId == app2.Id && x.Status == ConfigStatus.Enabled,
                publishedConfigUpdate);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(string appId)
    {
        var result = await repository.DeleteAsync(appId);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateAsync(App app, List<AppInheritanced>? appInheritanceds)
    {
        await repository.UpdateAsync(app);
        await appInheritancedRepository.DeleteAsync(x => x.AppId == app.Id);
        if (appInheritanceds != null)
        {
            await appInheritancedRepository.InsertAsync(appInheritanceds);
        }

        return true;
    }

    public async Task<bool> UpdateAsync(App app)
    {
        var result = await repository.UpdateAsync(app);
        return result.ModifiedCount > 0;
    }

    public Task<List<App>> GetAllAppsAsync()
    {
        return repository.SearchFor(x => true).ToListAsync();
    }

    public Task<List<App>> GetAllInheritancedAppsAsync()
    {
        return repository.SearchFor(x => x.Type == AppType.Inheritance).ToListAsync();
    }

    public Task<int> CountEnabledAppsAsync()
    {
        return repository.SearchFor(x => x.Enabled == true).CountAsync();
    }

    public async Task<List<App>> GetInheritancedAppsAsync(string appId)
    {
        var appInheritanceds =await appInheritancedRepository.SearchFor(x => x.AppId == appId)
            .OrderBy(x => x.Sort)
            .ToListAsync();
        
        var apps = new List<App>();

        foreach (var item in appInheritanceds)
        {
            var app = await GetAsync(item.InheritancedAppId);
            if (app is { Enabled: true })
            {
                apps.Add(app);
            }
        }

        return apps;
    }

    public async Task<List<App>> GetInheritancedFromAppsAsync(string appId)
    {
        var appInheritanceds = await appInheritancedRepository.SearchFor(a => a.InheritancedAppId == appId).ToListAsync();
        appInheritanceds = appInheritanceds.OrderBy(a => a.Sort).ToList();

        var apps = new List<App>();

        foreach (var item in appInheritanceds)
        {
            var app = await GetAsync(item.AppId);
            if (app is { Enabled: true })
            {
                apps.Add(app);
            }
        }

        return apps;
    }

    public async Task<bool> SaveUserAppAuth(string appId, List<string>? userIds, string permission)
    {
        var userAppAuthList = new List<UserAppAuth>();
        userIds ??= new List<string>();
        foreach (var userId in userIds)
        {
            userAppAuthList.Add(new UserAppAuth
            {
                Id = Guid.NewGuid().ToString("N"),
                AppId = appId,
                UserId = userId,
                Permission = permission
            });
        }
        await userAppAuthRepository.DeleteAsync(x => x.AppId == appId && x.Permission == permission);
        await userAppAuthRepository.InsertAsync(userAppAuthList);

        return true;
    }

    public async Task<List<User>> GetUserAppAuth(string appId, string permission)
    {
        var auths = await userAppAuthRepository.SearchFor(x => x.AppId == appId && x.Permission == permission).ToListAsync();

        var userIds = auths.Select(x => x.UserId).Distinct().ToList();
        return await userRepository.SearchFor(x => userIds.Contains(x.Id)).ToListAsync();
    }

    public List<string> GetAppGroups()
    {
        var groups = repository.SearchFor(x => true).GroupBy(x => x.Group).Select(x => x.Key);
        return groups.Where(x => !string.IsNullOrEmpty(x)).ToList();
    }
}