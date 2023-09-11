using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Freesql;
using System.Linq;

namespace AgileConfig.Server.Service
{
    public class AppService : IAppService
    {
        private FreeSqlContext _dbContext;

        public AppService(FreeSqlContext context)
        {
            _dbContext = context;
        }

        public async Task<bool> AddAsync(App app)
        {
            await _dbContext.Apps.AddAsync(app);
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }
        public async Task<bool> AddAsync(App app, List<AppInheritanced> appInheritanceds)
        {
            await _dbContext.Apps.AddAsync(app);
            if (appInheritanceds != null)
            {
                await _dbContext.AppInheritanceds.AddRangeAsync(appInheritanceds);
            }
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }
        public async Task<bool> DeleteAsync(App app)
        {
            app = await _dbContext.Apps.Where(a => a.Id == app.Id).ToOneAsync();
            if (app != null)
            {
                _dbContext.Apps.Remove(app);
                //怕有的同学误删app导致要恢复，所以保留配置项吧。
                var configs = await _dbContext.Configs.Where(x => x.AppId == app.Id).ToListAsync();
                foreach (var item in configs)
                {
                    item.Status = ConfigStatus.Deleted;
                    await _dbContext.UpdateAsync(item);
                }
                //删除发布的配置项
                var publishedConfigs = await _dbContext.ConfigPublished
                    .Where(x => x.AppId == app.Id && x.Status == ConfigStatus.Enabled)
                    .ToListAsync();
                foreach (var item in publishedConfigs)
                {
                    item.Status = ConfigStatus.Deleted;
                    await _dbContext.UpdateAsync(item);
                }
            }
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        public async Task<bool> DeleteAsync(string appId)
        {
            var app = await _dbContext.Apps.Where(a => a.Id == appId).ToOneAsync();
            if (app != null)
            {
                _dbContext.Apps.Remove(app);
            }
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        public async Task<App> GetAsync(string id)
        {
            return await _dbContext.Apps.Where(a => a.Id == id).ToOneAsync();
        }

        public async Task<List<App>> GetAllAppsAsync()
        {
            return await _dbContext.Apps.Where(a => 1 == 1).ToListAsync();
        }

        public async Task<bool> UpdateAsync(App app)
        {
            _dbContext.Update(app);
            var x = await _dbContext.SaveChangesAsync();

            var result = x > 0;

            return result;
        }

        public async Task<int> CountEnabledAppsAsync()
        {
            var q = await _dbContext.Apps.Where(a => a.Enabled == true).CountAsync();

            return (int)q;
        }

        public async Task<List<App>> GetAllInheritancedAppsAsync()
        {
            return await _dbContext.Apps.Where(a => a.Type == AppType.Inheritance).ToListAsync();
        }

        /// <summary>
        /// 根据appId查询所有继承的app
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<List<App>> GetInheritancedAppsAsync(string appId)
        {
            var appInheritanceds = await _dbContext.AppInheritanceds.Where(a => a.AppId == appId).ToListAsync();
            appInheritanceds = appInheritanceds.OrderBy(a => a.Sort).ToList();

            var apps = new List<App>();

            foreach (var item in appInheritanceds)
            {
                var app = await GetAsync(item.InheritancedAppId);
                if (app != null && app.Enabled)
                {
                    apps.Add(app);
                }
            }

            return apps;
        }

        /// <summary>
        /// 查询所有继承自该app的应用
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<List<App>> GetInheritancedFromAppsAsync(string appId)
        {
            var appInheritanceds = await _dbContext.AppInheritanceds.Where(a => a.InheritancedAppId == appId).ToListAsync();
            appInheritanceds = appInheritanceds.OrderBy(a => a.Sort).ToList();

            var apps = new List<App>();

            foreach (var item in appInheritanceds)
            {
                var app = await GetAsync(item.AppId);
                if (app != null && app.Enabled)
                {
                    apps.Add(app);
                }
            }

            return apps;
        }


        public async Task<bool> UpdateAsync(App app, List<AppInheritanced> appInheritanceds)
        {
            _dbContext.Update(app);
            var oldInheritancedApps = await _dbContext.AppInheritanceds.Where(a => a.AppId == app.Id).ToListAsync();
            _dbContext.RemoveRange(oldInheritancedApps);
            if (appInheritanceds != null)
            {
                await _dbContext.AddRangeAsync(appInheritanceds);
            }
            var x = await _dbContext.SaveChangesAsync();

            var result = x > 0;

            return result;
        }

        public async Task<bool> SaveUserAppAuth(string appId, List<string> userIds, string permission)
        {
            var userAppAuthList = new List<UserAppAuth>();
            if (userIds == null)
            {
                userIds = new List<string>();
            }
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
            await _dbContext.UserAppAuths.RemoveAsync(x => x.AppId == appId && x.Permission == permission);
            await _dbContext.UserAppAuths.AddRangeAsync(userAppAuthList);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public async Task<List<User>> GetUserAppAuth(string appId, string permission)
        {
            var auths = await _dbContext.UserAppAuths.Where(x => x.AppId == appId && x.Permission == permission).ToListAsync();

            var users = new List<User>();
            foreach (var auth in auths)
            {
                var user = await _dbContext.Users.Where(u => u.Id == auth.UserId).FirstAsync();
                if (user != null)
                {
                    users.Add(user);
                }
            }

            return users;
        }

        public List<string> GetAppGroups()
        {
            var groups = _dbContext.Apps.Select.GroupBy(x => x.Group).Select(x => x.Key)
                ;
            return groups.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
