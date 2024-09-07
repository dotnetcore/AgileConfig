using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Data.Abstraction;
using static FreeSql.Internal.GlobalFilter;

namespace AgileConfig.Server.Service
{
    public class AppService : IAppService
    {
        private readonly IAppRepository _appRepository;
        private readonly IAppInheritancedRepository _appInheritancedRepository;
        private readonly Func<string, IConfigRepository> _configRepositoryAccessor;
        private readonly Func<string, IConfigPublishedRepository> _configPublishedRepositoryAccessor;
        private readonly IUserRepository _userRepository;
        private readonly IUserAppAuthRepository _userAppAuthRepository;
        private readonly ISettingService _settingService;

        public AppService(
            IAppRepository repository,
            IAppInheritancedRepository appInheritancedRepository,
            Func<string, IConfigRepository> configRepository,
            Func<string, IConfigPublishedRepository> configPublishedRepository,
            IUserRepository userRepository,
            IUserAppAuthRepository userAppAuthRepository,
            ISettingService settingService
            )
        {
            _appRepository = repository;
            _appInheritancedRepository = appInheritancedRepository;
            _configRepositoryAccessor = configRepository;
            _configPublishedRepositoryAccessor = configPublishedRepository;
            _userRepository = userRepository;
            _userAppAuthRepository = userAppAuthRepository;
            _settingService = settingService;
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
                    
                var envs = await _settingService.GetEnvironmentList();
                var updatedConfigIds = new List<string>();
                var updatedConfigPublishedIds = new List<string>();

                foreach (var env in envs)
                {
                    using var configRepository = _configRepositoryAccessor(env);
                    using var configPublishedRepository = _configPublishedRepositoryAccessor(env);

                    //怕有的同学误删app导致要恢复，所以保留配置项吧。
                    var configs = await configRepository.QueryAsync(x => x.AppId == app.Id);
                    var waitDeleteConfigs = new List<Config>();
                    foreach (var item in configs)
                    {
                        if (updatedConfigIds.Contains(item.Id))
                        {
                            // 因为根据 env 构造的 provider 最终可能都定位到 default provider 上去，所以可能重复更新数据行，这里进行判断以下。
                            continue;
                        }
                        item.Status = ConfigStatus.Deleted;
                        waitDeleteConfigs.Add(item);
                        updatedConfigIds.Add(item.Id);
                    }
                    await configRepository.UpdateAsync(waitDeleteConfigs);
                    //删除发布的配置项
                    var publishedConfigs = await configPublishedRepository
                        .QueryAsync(x => x.AppId == app.Id && x.Status == ConfigStatus.Enabled)
                        ;
                    var waitDeletePublishedConfigs = new List<ConfigPublished>();
                    foreach (var item in publishedConfigs)
                    {
                        if (updatedConfigPublishedIds.Contains(item.Id))
                        {
                            // 因为根据 env 构造的 provider 最终可能都定位到 default provider 上去，所以可能重复更新数据行，这里进行判断以下。
                            continue;
                        }
                        item.Status = ConfigStatus.Deleted;
                        waitDeletePublishedConfigs.Add(item);
                        updatedConfigPublishedIds.Add(item.Id);
                    }
                    await configPublishedRepository.UpdateAsync(waitDeletePublishedConfigs);
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

        public Task<App> GetAsync(string id)
        {
            return _appRepository.GetAsync(id);
        }

        public Task<List<App>> GetAllAppsAsync()
        {
            return _appRepository.AllAsync();
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

        public Task<List<App>> GetAllInheritancedAppsAsync()
        {
            return _appRepository.QueryAsync(a => a.Type == AppType.Inheritance);
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
            var groups = apps.GroupBy(x => x.Group).Select(x => x.Key);
            return groups.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
