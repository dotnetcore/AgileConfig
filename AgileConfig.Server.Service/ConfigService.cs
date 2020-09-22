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

        public ConfigService(FreeSqlContext context, IMemoryCache memoryCache)
        {
            _dbContext = context;
            _memoryCache = memoryCache;
        }

        public async Task<bool> AddAsync(Config config)
        {
            await _dbContext.Configs.AddAsync(config);
            int x = await _dbContext.SaveChangesAsync();

            var result = x > 0;
            if (result)
            {
                ClearAppPublishedConfigsMd5Cache(config.AppId);
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

            string generateKey(Config config)
            {
                if (string.IsNullOrEmpty(config.Group))
                {
                    return config.Key;
                }

                return $"{config.Group}:{config.Key}";
            }

            var keyStr = string.Join('&', configs.Select(c => generateKey(c)).ToArray().OrderBy(k => k));
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

        private void ClearAppPublishedConfigsMd5Cache(string appId)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKey(appId);
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
            }

            return result;
        }
    }
}
