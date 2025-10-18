using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AgileConfig.Server.Data.Abstraction;

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

                    // Preserve configurations so they can be recovered if the app was deleted accidentally.
                    var configs =
                        await configRepository.QueryAsync(x => x.AppId == app.Id && x.Status == ConfigStatus.Enabled);
                    var waitDeleteConfigs = new List<Config>();
                    foreach (var item in configs)
                    {
                        if (updatedConfigIds.Contains(item.Id))
                        {
                            // Avoid duplicate updates when multiple environments use the same underlying provider.
                            continue;
                        }

                        item.Status = ConfigStatus.Deleted;
                        waitDeleteConfigs.Add(item);
                        updatedConfigIds.Add(item.Id);
                    }

                    await configRepository.UpdateAsync(waitDeleteConfigs);
                    // Delete published configuration entries.
                    var publishedConfigs = await configPublishedRepository
                            .QueryAsync(x => x.AppId == app.Id && x.Status == ConfigStatus.Enabled)
                        ;
                    var waitDeletePublishedConfigs = new List<ConfigPublished>();
                    foreach (var item in publishedConfigs)
                    {
                        if (updatedConfigPublishedIds.Contains(item.Id))
                        {
                            // Avoid duplicate updates when multiple environments use the same underlying provider.
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

        public async Task<(List<App> Apps, long Count)> SearchAsync(string id, string name, string group,
            string sortField, string ascOrDesc,
            int current, int pageSize)
        {
            Expression<Func<App, bool>> exp = app => true;

            if (!string.IsNullOrWhiteSpace(id))
            {
                exp = exp.And(a => a.Id.Contains(id));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                exp = exp.And(a => a.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(group))
            {
                exp = exp.And(a => a.Group == group);
            }

            if (string.IsNullOrWhiteSpace(ascOrDesc))
            {
                ascOrDesc = "asc";
            }

            var apps = await _appRepository.QueryPageAsync(exp, current, pageSize, sortField,
                ascOrDesc?.StartsWith("asc") ?? true ? "ASC" : "DESC");
            var count = await _appRepository.CountAsync(exp);

            return (apps, count);
        }

        public async Task<(List<GroupedApp> GroupedApps, long Count)> SearchGroupedAsync(string id, string name,
            string group, string sortField, string ascOrDesc, int current,
            int pageSize)
        {
            Expression<Func<App, bool>> exp = app => true;

            if (!string.IsNullOrWhiteSpace(id))
            {
                exp = exp.And(a => a.Id.Contains(id));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                exp = exp.And(a => a.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(group))
            {
                exp = exp.And(a => a.Group == group);
            }

            var apps = await _appRepository.QueryAsync(exp);

            var appGroups = apps.GroupBy(x => x.Group);
            var appGroupList = new List<GroupedApp>();
            foreach (var appGroup in appGroups)
            {
                var app = appGroup.First();
                var firstGroup = new GroupedApp()
                {
                    App = app
                };
                var children = new List<GroupedApp>();
                if (appGroup.Count() > 1)
                {
                    foreach (var item in appGroup)
                    {
                        if (firstGroup.App.Id != item.Id)
                        {
                            children.Add(new GroupedApp()
                            {
                                App = item
                            });
                        }
                    }
                }

                if (children.Count > 0)
                {
                    firstGroup.Children = children;
                }

                appGroupList.Add(firstGroup);
            }

            var sortProperty = new Dictionary<string, PropertyInfo>()
            {
                { "id", typeof(App).GetProperty("Id") },
                { "name", typeof(App).GetProperty("Name") },
                { "group", typeof(App).GetProperty("Group") },
                { "createTime", typeof(App).GetProperty("CreateTime") }
            };

            if (sortProperty.TryGetValue(sortField, out var propertyInfo))
            {
                appGroupList = ascOrDesc?.StartsWith("asc") ?? true
                    ? appGroupList.OrderBy(x => propertyInfo.GetValue(x.App, null)).ToList()
                    : appGroupList.OrderByDescending(x => propertyInfo.GetValue(x.App, null)).ToList();
            }

            var page = appGroupList.Skip(current - 1 * pageSize).Take(pageSize).ToList();

            return (page, appGroupList.Count);
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
        /// Retrieve all applications that this app inherits from.
        /// </summary>
        /// <param name="appId">Application ID whose inheritance chain should be resolved.</param>
        /// <returns>List of applications inherited by the specified app.</returns>
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
        /// Retrieve applications that inherit from the specified app.
        /// </summary>
        /// <param name="appId">Application ID whose dependents should be returned.</param>
        /// <returns>List of applications inheriting from the specified app.</returns>
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

            var removeApps =
                await _userAppAuthRepository.QueryAsync(x => x.AppId == appId && x.Permission == permission);
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