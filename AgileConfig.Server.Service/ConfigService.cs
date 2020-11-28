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
            return await _dbContext.Configs.Where(c =>
                c.AppId == appId &&
                c.Key == key &&
                c.Group == group &&
                c.Status == ConfigStatus.Enabled
            ).FirstAsync();
        }

        public async Task<List<Config>> GetByAppId(string appId)
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
            if (_memoryCache.TryGetValue(cacheKey, out string md5))
            {
                return md5;
            }

            md5 = await AppPublishedConfigsMd5(appId);

            var cacheOp = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _memoryCache.Set(cacheKey, md5, cacheOp);

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
            _memoryCache.Remove(cacheKey);
        }
        private void ClearAppPublishedConfigsMd5CacheWithInheritanced(string appId)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKeyWithInheritanced(appId);
            _memoryCache.Remove(cacheKey);
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
            if (_memoryCache.TryGetValue(cacheKey, out string md5))
            {
                return md5;
            }

            md5 = await AppPublishedConfigsMd5WithInheritanced(appId);

            var cacheOp = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _memoryCache.Set(cacheKey, md5, cacheOp);

            return md5;
        }
    }
}
