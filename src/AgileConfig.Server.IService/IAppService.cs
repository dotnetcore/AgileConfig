﻿using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public class GroupedApp
    {
        public App App { get; set; }
        public List<GroupedApp> Children { get; set; }
    }
    public interface IAppService : IDisposable
    {
        Task<App> GetAsync(string id);
        Task<bool> AddAsync(App app);
        Task<bool> AddAsync(App app, List<AppInheritanced> appInheritanceds);

        Task<bool> DeleteAsync(App app);

        Task<bool> DeleteAsync(string appId);

        Task<bool> UpdateAsync(App app, List<AppInheritanced> appInheritanceds);
        Task<bool> UpdateAsync(App app);

        Task<List<App>> GetAllAppsAsync();

        Task<(List<App> Apps, long Count)> SearchAsync(string id, string name, string group,string sortField, string ascOrDesc,
          int current, int pageSize);
        
        Task<(List<GroupedApp> GroupedApps, long Count)> SearchGroupedAsync(string id, string name, string group,string sortField, string ascOrDesc,
            int current, int pageSize);
        
        Task<List<App>> GetAllInheritancedAppsAsync();

        Task<int> CountEnabledAppsAsync();

        Task<List<App>> GetInheritancedAppsAsync(string appId);

        Task<List<App>> GetInheritancedFromAppsAsync(string appId);

        Task<bool> SaveUserAppAuth(string appId, List<string> userIds, string permission);

        Task<List<User>> GetUserAppAuth(string appId, string permission);

        Task<List<string>> GetAppGroups();
    }
}
