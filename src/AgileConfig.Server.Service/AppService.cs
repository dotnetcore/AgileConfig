using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.Service
{
    public class AppService : IAppService
    {
        private readonly IAppRepository _appRepository;
        private readonly IAppInheritancedRepository _appInheritancedRepository;
        private readonly IConfigRepository _configRepository;
        private readonly IConfigPublishedRepository _configPublishedRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserAppAuthRepository _userAppAuthRepository;

        public AppService(
            IAppRepository repository, 
            IAppInheritancedRepository appInheritancedRepository,
            IConfigRepository configRepository,
            IConfigPublishedRepository configPublishedRepository,
            IUserRepository userRepository,
            IUserAppAuthRepository userAppAuthRepository)
        {
            _appRepository = repository;
            _appInheritancedRepository = appInheritancedRepository;
            _configRepository = configRepository;
            _configPublishedRepository = configPublishedRepository;
            _userRepository = userRepository;
            _userAppAuthRepository = userAppAuthRepository;
        }

        public async Task<bool> AddAsync(App app)
        {
            await _appRepository.InsertAsync(app);

            return true;
        }
        public async Task<bool> AddAsync(App app, List<AppInheritanced> appInheritanceds)
        {
            await _appRepository.InsertAsync(app);
            if (appInheritanceds != null)
            {
                await _appInheritancedRepository.InsertAsync(appInheritanceds);
            }

            return true;
        }
        public async Task<bool> DeleteAsync(App app)
        {
            app = await _appRepository.GetAsync(app.Id);
            if (app != null)
            {
                await _appRepository.DeleteAsync(app);
                //怕有的同学误删app导致要恢复，所以保留配置项吧。
                var configs = await _configRepository.QueryAsync(x => x.AppId == app.Id);
                foreach (var item in configs)
                {
                    item.Status = ConfigStatus.Deleted;
                    await _configRepository.UpdateAsync(item);
                }
                //删除发布的配置项
                var publishedConfigs = await _configPublishedRepository
                    .QueryAsync(x => x.AppId == app.Id && x.Status == ConfigStatus.Enabled)
                    ;
                foreach (var item in publishedConfigs)
                {
                    item.Status = ConfigStatus.Deleted;
                    await _configPublishedRepository.UpdateAsync(item);
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(string appId)
        {
            var app = await _appRepository.GetAsync(appId);
            if (app != null)
            {
               await _appRepository.DeleteAsync(app);
            }

            return true;
        }

        public async Task<App> GetAsync(string id)
        {
            return await _appRepository.GetAsync(id);
        }

        public async Task<List<App>> GetAllAppsAsync()
        {
            return await _appRepository.AllAsync();
        }

        public async Task<bool> UpdateAsync(App app)
        {
            await _appRepository.UpdateAsync(app);

            return true;
        }

        public async Task<int> CountEnabledAppsAsync()
        {
            var q = await _appRepository.QueryAsync(a => a.Enabled == true);

            return q.Count;
        }

        public async Task<List<App>> GetAllInheritancedAppsAsync()
        {
            return await _appRepository.QueryAsync(a => a.Type == AppType.Inheritance);
        }

        /// <summary>
        /// 根据appId查询所有继承的app
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<List<App>> GetInheritancedAppsAsync(string appId)
        {
            var appInheritanceds = await _appInheritancedRepository.QueryAsync(a => a.AppId == appId);
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
            var appInheritanceds = await _appInheritancedRepository.QueryAsync(a => a.InheritancedAppId == appId);
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
            await _appRepository.UpdateAsync(app);
            var oldInheritancedApps = await _appInheritancedRepository.QueryAsync(a => a.AppId == app.Id);
            await _appInheritancedRepository.DeleteAsync(oldInheritancedApps);
            if (appInheritanceds != null)
            {
                await _appInheritancedRepository.InsertAsync(appInheritanceds);
            }

            return true;
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
            var removeApps = await _userAppAuthRepository.QueryAsync(x => x.AppId == appId && x.Permission == permission);
            await _userAppAuthRepository.DeleteAsync(removeApps);
            await _userAppAuthRepository.InsertAsync(userAppAuthList);

            return true;
        }

        public void Dispose()
        {
            _appInheritancedRepository.Dispose();
            _appRepository.Dispose();
            _configPublishedRepository.Dispose();
            _configRepository.Dispose();
            _userAppAuthRepository.Dispose();
            _userRepository.Dispose();
        }

        public async Task<List<User>> GetUserAppAuth(string appId, string permission)
        {
            var auths = await _userAppAuthRepository.QueryAsync(x => x.AppId == appId && x.Permission == permission);

            var users = new List<User>();
            foreach (var auth in auths)
            {
                var user = await _userRepository.GetAsync(auth.UserId);
                if (user != null)
                {
                    users.Add(user);
                }
            }

            return users;
        }

        public async Task<List<string>> GetAppGroups()
        {
            var apps = await _appRepository.AllAsync();
            var groups = apps.GroupBy(x => x.Group).Select(x => x.Key)
                ;
            return groups.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
