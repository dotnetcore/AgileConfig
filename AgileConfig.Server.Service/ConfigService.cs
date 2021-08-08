using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Service
{
    public class ConfigService : IConfigService
    {
        private readonly FreeSqlContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IAppService _appService;

        public ConfigService(FreeSqlContext context, IMemoryCache memoryCache, IAppService appService)
        {
            _dbContext = context;
            _memoryCache = memoryCache;
            _appService = appService;
        }

        public async Task<bool> AddAsync(Config config)
        {
            await _dbContext.Configs.AddAsync(config);
            int x = await _dbContext.SaveChangesAsync();

            var result = x > 0;
            if (result)
            {
                ClearAppPublishedConfigsMd5Cache(config.AppId);
                ClearAppPublishedConfigsMd5CacheWithInheritanced(config.AppId);
            }

            return result;
        }
        public async Task<bool> UpdateAsync(Config config)
        {
            _dbContext.Update(config);
            var x = await _dbContext.SaveChangesAsync();

            var result = x > 0;
            if (result)
            {
                ClearAppPublishedConfigsMd5Cache(config.AppId);
                ClearAppPublishedConfigsMd5CacheWithInheritanced(config.AppId);
            }

            return result;
        }

        public async Task<bool> DeleteAsync(Config config)
        {
            config = await _dbContext.Configs.Where(c => c.Id == config.Id).ToOneAsync();
            if (config != null)
            {
                _dbContext.Configs.Remove(config);
            }
            int x = await _dbContext.SaveChangesAsync();

            var result = x > 0;
            if (result)
            {
                ClearAppPublishedConfigsMd5Cache(config.AppId);
                ClearAppPublishedConfigsMd5CacheWithInheritanced(config.AppId);
            }

            return result;
        }

        public async Task<bool> DeleteAsync(string configId)
        {
            var config = await _dbContext.Configs.Where(c => c.Id == configId).ToOneAsync();
            if (config != null)
            {
                _dbContext.Configs.Remove(config);
            }
            int x = await _dbContext.SaveChangesAsync();

            var result = x > 0;
            if (result)
            {
                ClearAppPublishedConfigsMd5Cache(config.AppId);
                ClearAppPublishedConfigsMd5CacheWithInheritanced(config.AppId);
            }

            return result;
        }

        public async Task<Config> GetAsync(string id)
        {
            var config = await _dbContext.Configs.Where(c => c.Id == id).ToOneAsync();

            return config;
        }

        public async Task<List<Config>> GetAllConfigsAsync()
        {
            return await _dbContext.Configs.Where(c => c.Status == ConfigStatus.Enabled).ToListAsync();
        }

        public async Task<Config> GetByAppIdKey(string appId, string group, string key)
        {
            var q = _dbContext.Configs.Where(c =>
                 c.AppId == appId &&
                 c.Key == key &&
                 c.Status == ConfigStatus.Enabled
            );
            if (string.IsNullOrEmpty(group))
            {
                q = q.Where(c => c.Group == "" || c.Group == null);
            }
            else
            {
                q = q.Where(c => c.Group == group);

            }
            return await q.FirstAsync();
        }

        public async Task<List<Config>> GetByAppIdAsync(string appId)
        {
            return await _dbContext.Configs.Where(c =>
                c.AppId == appId && c.Status == ConfigStatus.Enabled
            ).ToListAsync();
        }

        public async Task<List<Config>> Search(string appId, string group, string key)
        {
            var q = _dbContext.Configs.Where(c => c.Status == ConfigStatus.Enabled);
            if (!string.IsNullOrEmpty(appId))
            {
                q = q.Where(c => c.AppId == appId);
            }
            if (!string.IsNullOrEmpty(group))
            {
                q = q.Where(c => c.Group.Contains(group));
            }
            if (!string.IsNullOrEmpty(key))
            {
                q = q.Where(c => c.Key.Contains(key));
            }

            return await q.ToListAsync();
        }

        public async Task<int> CountEnabledConfigsAsync()
        {
            var q = await _dbContext.Configs.Where(c => c.Status == ConfigStatus.Enabled).CountAsync();

            return (int)q;
        }

        public string GenerateKey(Config config)
        {
            if (string.IsNullOrEmpty(config.Group))
            {
                return config.Key;
            }

            return $"{config.Group}:{config.Key}";
        }

        /// <summary>
        /// 获取当前app的配置集合的md5版本
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<string> AppPublishedConfigsMd5(string appId)
        {
            var configs = await _dbContext.Configs.Where(c =>
                c.AppId == appId && c.Status == ConfigStatus.Enabled && c.OnlineStatus == OnlineStatus.Online
            ).ToListAsync();

            var keyStr = string.Join('&', configs.Select(c => GenerateKey(c)).ToArray().OrderBy(k => k));
            var valStr = string.Join('&', configs.Select(c => c.Value).ToArray().OrderBy(v => v));
            var txt = $"{keyStr}&{valStr}";

            return Encrypt.Md5(txt);
        }

        /// <summary>
        /// 获取当前app的配置集合的md5版本，1分钟缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<string> AppPublishedConfigsMd5Cache(string appId)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKey(appId);
            if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out string md5))
            {
                return md5;
            }

            md5 = await AppPublishedConfigsMd5(appId);

            var cacheOp = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _memoryCache?.Set(cacheKey, md5, cacheOp);

            return md5;
        }

        private string AppPublishedConfigsMd5CacheKey(string appId)
        {
            return $"ConfigService_AppPublishedConfigsMd5Cache_{appId}";
        }

        private string AppPublishedConfigsMd5CacheKeyWithInheritanced(string appId)
        {
            return $"ConfigService_AppPublishedConfigsMd5CacheWithInheritanced_{appId}";
        }

        private void ClearAppPublishedConfigsMd5Cache(string appId)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKey(appId);
            _memoryCache?.Remove(cacheKey);
        }
        private void ClearAppPublishedConfigsMd5CacheWithInheritanced(string appId)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKeyWithInheritanced(appId);
            _memoryCache?.Remove(cacheKey);
        }

        public async Task<List<Config>> GetPublishedConfigsByAppId(string appId)
        {
            return await _dbContext.Configs.Where(c =>
                 c.AppId == appId && c.Status == ConfigStatus.Enabled && c.OnlineStatus == OnlineStatus.Online
             ).ToListAsync();
        }

        public async Task<bool> AddRangeAsync(List<Config> configs)
        {
            await _dbContext.Configs.AddRangeAsync(configs);
            int x = await _dbContext.SaveChangesAsync();

            var result = x > 0;
            if (result)
            {
                ClearAppPublishedConfigsMd5Cache(configs.First().AppId);
                ClearAppPublishedConfigsMd5CacheWithInheritanced(configs.First().AppId);
            }

            return result;
        }

        /// <summary>
        /// 获取app的配置项继承的app配置合并进来
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<List<Config>> GetPublishedConfigsByAppIdWithInheritanced(string appId)
        {
            var configs = await GetPublishedConfigsByAppIdWithInheritanced_Dictionary(appId);

            return configs.Values.ToList();
        }

        /// <summary>
        /// 获取app的配置项继承的app配置合并进来转换为字典
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, Config>> GetPublishedConfigsByAppIdWithInheritanced_Dictionary(string appId)
        {
            var apps = new List<string>();
            var inheritanceApps = await _appService.GetInheritancedAppsAsync(appId);
            for (int i = 0; i < inheritanceApps.Count; i++)
            {
                if (inheritanceApps[i].Enabled)
                {
                    apps.Add(inheritanceApps[i].Id);//后继承的排在后面
                }
            }
            apps.Add(appId);//本应用放在最后

            var configs = new Dictionary<string, Config>();
            //读取所有继承的配置跟本app的配置
            for (int i = 0; i < apps.Count; i++)
            {
                var id = apps[i];
                var publishConfigs = await GetPublishedConfigsByAppId(id);
                for (int j = 0; j < publishConfigs.Count; j++)
                {
                    var config = publishConfigs[j];
                    var key = GenerateKey(config);
                    if (configs.ContainsKey(key))
                    {
                        //后面的覆盖前面的
                        configs[key] = config;
                    }
                    else
                    {
                        configs.Add(key, config);
                    }
                }
            }

            return configs;
        }

        public async Task<string> AppPublishedConfigsMd5WithInheritanced(string appId)
        {
            var configs = await GetPublishedConfigsByAppIdWithInheritanced(appId);

            var keyStr = string.Join('&', configs.Select(c => GenerateKey(c)).ToArray().OrderBy(k => k));
            var valStr = string.Join('&', configs.Select(c => c.Value).ToArray().OrderBy(v => v));
            var txt = $"{keyStr}&{valStr}";

            return Encrypt.Md5(txt);
        }

        /// <summary>
        /// 获取当前app的配置集合的md5版本，1分钟缓存 集合了继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<string> AppPublishedConfigsMd5CacheWithInheritanced(string appId)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKeyWithInheritanced(appId);
            if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out string md5))
            {
                return md5;
            }

            md5 = await AppPublishedConfigsMd5WithInheritanced(appId);

            var cacheOp = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _memoryCache?.Set(cacheKey, md5, cacheOp);

            return md5;
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _appService?.Dispose();
        }

        private static readonly object Lockobj = new object();

        /// <summary>
        /// 发布当前待发布的配置项
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="operatorr">操作员</param>
        /// <returns></returns>
        public (bool result, string publishTimelineId) Publish(string appId, string operatorr)
        {
            lock (Lockobj)
            {
                var waitPublishConfigs = _dbContext.Configs.Where(x =>
                    x.Status == ConfigStatus.Enabled &&
                    x.EditStatus != EditStatus.Commit).ToList();
                //这里默认admin console 实例只部署一个，如果部署多个同步操作，这个version会有问题
                var versionMax = _dbContext.PublishTimeline.Select.Where(x=>x.AppId == appId).Max(x => x.Version);

                var publishTimelineNode = new PublishTimeline();
                publishTimelineNode.AppId = appId;
                publishTimelineNode.Id = Guid.NewGuid().ToString("N");
                publishTimelineNode.PublishTime = DateTime.Now;
                publishTimelineNode.PublishUserId = operatorr;
                publishTimelineNode.Version = versionMax + 1;

                var publishDetails = new List<PublishDetail>();

                waitPublishConfigs.ForEach(x =>
                {
                    publishDetails.Add(new PublishDetail()
                    {
                        AppId = appId,
                        ConfigId = x.Id,
                        Description = x.Description,
                        EditStatus = x.EditStatus,
                        Group = x.Group,
                        Id = Guid.NewGuid().ToString("N"),
                        Key = x.Key,
                        Value = x.Value,
                        PublishTimelineId = publishTimelineNode.Id,
                        Version = publishTimelineNode.Version
                    });

                    if (x.EditStatus == EditStatus.Deleted)
                    {
                        x.OnlineStatus = OnlineStatus.WaitPublish;
                        x.Status = ConfigStatus.Deleted;
                    }
                    else
                    {
                        x.OnlineStatus = OnlineStatus.Online;
                        x.Status = ConfigStatus.Enabled;
                    }
                    x.EditStatus = EditStatus.Commit;
                    x.OnlineStatus = OnlineStatus.Online;
                });

                //当前发布的配置
                var publishedConfigs = _dbContext.ConfigPublished.Where(x => x.Status == ConfigStatus.Enabled && x.AppId == appId).ToList();
                //复制一份新版本，最后插入发布表
                var publishedConfigsCopy = new List<ConfigPublished>();
                publishedConfigs.ForEach(x =>
                {
                    publishedConfigsCopy.Add(new ConfigPublished()
                    {
                        AppId = x.AppId,
                        ConfigId = x.ConfigId,
                        Group = x.Group,
                        Id = Guid.NewGuid().ToString("N"),
                        Key = x.Key,
                        PublishTimelineId = publishTimelineNode.Id,
                        PublishTime = publishTimelineNode.PublishTime,
                        Status = ConfigStatus.Enabled,
                        Version = publishTimelineNode.Version,
                        Value = x.Value
                    });
                    x.Status = ConfigStatus.Deleted;
                });

                publishDetails.ForEach(x =>
                {
                    if (x.EditStatus == EditStatus.Add)
                    {
                        publishedConfigsCopy.Add(new ConfigPublished()
                        {
                            AppId = x.AppId,
                            ConfigId = x.ConfigId,
                            Group = x.Group,
                            Id = Guid.NewGuid().ToString("N"),
                            Key = x.Key,
                            PublishTimelineId = publishTimelineNode.Id,
                            PublishTime = publishTimelineNode.PublishTime,
                            Status = ConfigStatus.Enabled,
                            Value = x.Value,
                            Version = publishTimelineNode.Version
                        });
                    }
                    if (x.EditStatus == EditStatus.Edit)
                    {
                        var oldEntity = publishedConfigsCopy.FirstOrDefault(c => c.ConfigId == x.ConfigId);
                        if (oldEntity == null)
                        {
                            //do nothing
                        }
                        else
                        {
                            //edit
                            oldEntity.Version = publishTimelineNode.Version;
                            oldEntity.Group = x.Group;
                            oldEntity.Key = x.Key;
                            oldEntity.Value = x.Value;
                            oldEntity.PublishTime = publishTimelineNode.PublishTime;
                        }
                    }
                    if (x.EditStatus == EditStatus.Deleted)
                    {
                        var oldEntity = publishedConfigsCopy.FirstOrDefault(c => c.ConfigId == x.ConfigId);
                        if (oldEntity == null)
                        {
                            //do nothing
                        }
                        else
                        {
                            //remove
                            publishedConfigsCopy.Remove(oldEntity);
                        }
                    }
                });

                _dbContext.Configs.UpdateRange(waitPublishConfigs);
                _dbContext.PublishTimeline.Add(publishTimelineNode);
                _dbContext.PublishDetail.AddRange(publishDetails);
                _dbContext.ConfigPublished.UpdateRange(publishedConfigs);
                _dbContext.ConfigPublished.AddRange(publishedConfigsCopy);

                var result = _dbContext.SaveChanges();

                return (result > 0, publishTimelineNode.Id);
            }
        }

        public async Task<bool> IsPublishedAsync(string configId)
        {
           var any = await _dbContext.ConfigPublished.Select.AnyAsync(
                x => x.ConfigId == configId && x.Status == ConfigStatus.Enabled);

           return any;
        }

        public async Task<List<PublishDetail>> GetPublishDetailByPublishTimelineId(string publishTimelineId)
        {
            var list = await _dbContext.PublishDetail.Where(x => x.PublishTimelineId == publishTimelineId).ToListAsync();

            return list;
        }

        public async Task<PublishTimeline> GetPublishTimeLineNode(string publishTimelineId)
        {
            var one = await _dbContext.PublishTimeline.Where(x => x.Id == publishTimelineId).FirstAsync();

            return one;
        }
    }
}
