using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using System.Linq.Expressions;
using System.Threading;

namespace AgileConfig.Server.Service
{
    public class ConfigService : IConfigService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAppService _appService;
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly Func<string, IUow> _uowAccessor;
        private readonly Func<string, IConfigRepository> _configRepositoryAccessor;
        private readonly Func<string, IConfigPublishedRepository> _configPublishedRepositoryAccessor;
        private readonly Func<string, IPublishDetailRepository> _publishDetailRepositoryAccessor;
        private readonly Func<string, IPublishTimelineRepository> _publishTimelineRepositoryAccsssor;

        public ConfigService(IMemoryCache memoryCache,
            IAppService appService,
            ISettingService settingService,
            IUserService userService,
            Func<string, IUow> uowAccessor,
            Func<string, IConfigRepository> configRepositoryAccessor,
            Func<string, IConfigPublishedRepository> configPublishedRepositoryAccessor,
            Func<string, IPublishDetailRepository> publishDetailRepositoryAccessor,
            Func<string, IPublishTimelineRepository> publishTimelineRepositoryAccessor)
        {
            _memoryCache = memoryCache;
            _appService = appService;
            _settingService = settingService;
            _userService = userService;
            _uowAccessor = uowAccessor;
            _configRepositoryAccessor = configRepositoryAccessor;
            _configPublishedRepositoryAccessor = configPublishedRepositoryAccessor;
            _publishDetailRepositoryAccessor = publishDetailRepositoryAccessor;
            _publishTimelineRepositoryAccsssor = publishTimelineRepositoryAccessor;
        }

        public async Task<bool> AddAsync(Config config, string env)
        {
            if (config.Value == null)
            {
                config.Value = "";
            }

            using var repoistory = _configRepositoryAccessor(env);
            await repoistory.InsertAsync(config);

            return true;
        }

        public async Task<bool> UpdateAsync(Config config, string env)
        {
            using var repoistory = _configRepositoryAccessor(env);
            await repoistory.UpdateAsync(config);

            return true;
        }

        public async Task<bool> UpdateAsync(List<Config> configs, string env)
        {
            using var repoistory = _configRepositoryAccessor(env);
            foreach (var item in configs)
            {
                await repoistory.UpdateAsync(item);
            }

            return true;
        }

        public async Task<bool> CancelEdit(List<string> ids, string env)
        {
            using var configRepository = _configRepositoryAccessor(env);
            foreach (var configId in ids)
            {
                var config = await configRepository.GetAsync(configId);
                ;
                if (config == null)
                {
                    throw new Exception("Can not find config by id " + configId);
                }

                if (config.EditStatus == EditStatus.Commit)
                {
                    continue;
                }

                if (config.EditStatus == EditStatus.Add)
                {
                    await configRepository.DeleteAsync(config);
                }

                if (config.EditStatus == EditStatus.Deleted || config.EditStatus == EditStatus.Edit)
                {
                    config.OnlineStatus = OnlineStatus.Online;
                    config.EditStatus = EditStatus.Commit;
                    config.UpdateTime = DateTime.Now;

                    var publishedConfig = await GetPublishedConfigAsync(configId, env);
                    if (publishedConfig == null)
                    {
                        //
                        throw new Exception("Can not find published config by id " + configId);
                    }
                    else
                    {
                        //reset value
                        config.Value = publishedConfig.Value;
                        config.OnlineStatus = OnlineStatus.Online;
                    }

                    await configRepository.UpdateAsync(config);
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(Config config, string env)
        {
            using var configRepository = _configRepositoryAccessor(env);
            config = await configRepository.GetAsync(config.Id);
            if (config != null)
            {
                await configRepository.DeleteAsync(config);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(string configId, string env)
        {
            using var configRepository = _configRepositoryAccessor(env);
            var config = await configRepository.GetAsync(configId);
            if (config != null)
            {
                await configRepository.DeleteAsync(config);
            }

            return true;
        }

        public async Task<Config> GetAsync(string id, string env)
        {
            using var repository = _configRepositoryAccessor(env);
            var config = await repository.GetAsync(id);

            return config;
        }

        public async Task<List<Config>> GetAllConfigsAsync(string env)
        {
            using var repository = _configRepositoryAccessor(env);
            return await repository.QueryAsync(c => c.Status == ConfigStatus.Enabled && c.Env == env);
        }

        public async Task<Config> GetByAppIdKeyEnv(string appId, string group, string key, string env)
        {
            Expression<Func<Config, bool>> exp = (c) => c.AppId == appId &&
             c.Key == key &&
             c.Env == env &&
             c.Status == ConfigStatus.Enabled;

            if (string.IsNullOrEmpty(group))
            {
                Expression<Func<Config, bool>> exp1 = c => c.Group == "" || c.Group == null;
                exp.And(exp1);
            }
            else
            {
                Expression<Func<Config, bool>> exp1 = c => c.Group == group;
                exp.And(exp1);
            }

            using var repository = _configRepositoryAccessor(env);
            var configs = await repository.QueryAsync(exp);

            return configs.FirstOrDefault();
        }

        public async Task<List<Config>> GetByAppIdAsync(string appId, string env)
        {
            using var repository = _configRepositoryAccessor(env);
            return await repository.QueryAsync(c =>
                c.AppId == appId && c.Status == ConfigStatus.Enabled && c.Env == env
            );
        }

        public async Task<List<Config>> Search(string appId, string group, string key, string env)
        {
            using var repository = _configRepositoryAccessor(env);

            Expression<Func<Config, bool>> exp = c => c.Status == ConfigStatus.Enabled && c.Env == env;
            if (!string.IsNullOrEmpty(appId))
            {
                exp = exp.And(c => c.AppId == appId);
            }

            if (!string.IsNullOrEmpty(group))
            {
                exp = exp.And(c => c.Group.Contains(group));
            }

            if (!string.IsNullOrEmpty(key))
            {
                exp = exp.And(c => c.Key.Contains(key));
            }

            return await repository.QueryAsync(exp);
        }

        public async Task<int> CountEnabledConfigsAsync()
        {
            int count = 0;
            var envs = await _settingService.GetEnvironmentList();
            foreach (var e in envs)
            {
                count += await CountEnabledConfigsAsync(e);
            }

            return count;
        }

        public async Task<int> CountEnabledConfigsAsync(string env)
        {
            //这里计算所有的配置
            using var repository = _configRepositoryAccessor(env);
            var q = await repository.QueryAsync(c => c.Status == ConfigStatus.Enabled && c.Env == env);

            return q.Count;
        }

        public string GenerateKey(Config config)
        {
            if (string.IsNullOrEmpty(config.Group))
            {
                return config.Key;
            }

            return $"{config.Group}:{config.Key}";
        }

        public string GenerateKey(ConfigPublished config)
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
        public async Task<string> AppPublishedConfigsMd5(string appId, string env)
        {
            using var repository = _configPublishedRepositoryAccessor(env);
            var configs = await repository.QueryAsync(c =>
                c.AppId == appId && c.Status == ConfigStatus.Enabled
                                 && c.Env == env
            );

            var keyStr = string.Join('&', configs.Select(c => GenerateKey(c)).ToArray().OrderBy(k => k, StringComparer.Ordinal));
            var valStr = string.Join('&', configs.Select(c => c.Value).ToArray().OrderBy(v => v, StringComparer.Ordinal));
            var txt = $"{keyStr}&{valStr}";

            return Encrypt.Md5(txt);
        }

        /// <summary>
        /// 获取当前app的配置集合的md5版本，1分钟缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<string> AppPublishedConfigsMd5Cache(string appId, string env)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKey(appId, env);
            if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out string md5))
            {
                return md5;
            }

            md5 = await AppPublishedConfigsMd5(appId, env);

            var cacheOp = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _memoryCache?.Set(cacheKey, md5, cacheOp);

            return md5;
        }

        private string AppPublishedConfigsMd5CacheKey(string appId, string env)
        {
            return $"ConfigService_AppPublishedConfigsMd5Cache_{appId}_{env}";
        }

        private string AppPublishedConfigsMd5CacheKeyWithInheritanced(string appId, string env)
        {
            return $"ConfigService_AppPublishedConfigsMd5CacheWithInheritanced_{appId}_{env}";
        }

        private void ClearAppPublishedConfigsMd5Cache(string appId, string env)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKey(appId, env);
            _memoryCache?.Remove(cacheKey);
        }

        private void ClearAppPublishedConfigsMd5CacheWithInheritanced(string appId, string env)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKeyWithInheritanced(appId, env);
            _memoryCache?.Remove(cacheKey);
        }

        public async Task<bool> AddRangeAsync(List<Config> configs, string env)
        {
            configs.ForEach(x =>
            {
                if (x.Value == null)
                {
                    x.Value = "";
                }
            });

            using var repository = _configRepositoryAccessor(env);
            await repository.InsertAsync(configs);

            ClearAppPublishedConfigsMd5Cache(configs.First().AppId, env);
            ClearAppPublishedConfigsMd5CacheWithInheritanced(configs.First().AppId, env);

            return true;
        }

        /// <summary>
        /// 获取app的配置项继承的app配置合并进来
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<List<Config>> GetPublishedConfigsByAppIdWithInheritanced(string appId, string env)
        {
            var configs = await GetPublishedConfigsByAppIdWithInheritanced_Dictionary(appId, env);

            return configs.Values.ToList();
        }

        /// <summary>
        /// 获取app的配置项继承的app配置合并进来转换为字典
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, Config>> GetPublishedConfigsByAppIdWithInheritanced_Dictionary(
            string appId, string env)
        {
            var apps = new List<string>();
            var inheritanceApps = await _appService.GetInheritancedAppsAsync(appId);
            for (int i = 0; i < inheritanceApps.Count; i++)
            {
                if (inheritanceApps[i].Enabled)
                {
                    apps.Add(inheritanceApps[i].Id as string); //后继承的排在后面
                }
            }

            apps.Add(appId); //本应用放在最后

            var configs = new Dictionary<string, Config>();
            //读取所有继承的配置跟本app的配置
            for (int i = 0; i < apps.Count; i++)
            {
                var id = apps[i];
                var publishConfigs = await GetPublishedConfigsAsync(id, env);
                for (int j = 0; j < publishConfigs.Count; j++)
                {
                    var config = publishConfigs[j].Convert();
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

        public async Task<string> AppPublishedConfigsMd5WithInheritanced(string appId, string env)
        {
            var configs = await GetPublishedConfigsByAppIdWithInheritanced(appId, env);

            var keyStr = string.Join('&', configs.Select(c => GenerateKey(c)).ToArray().OrderBy(k => k, StringComparer.Ordinal));
            var valStr = string.Join('&', configs.Select(c => c.Value).ToArray().OrderBy(v => v, StringComparer.Ordinal));
            var txt = $"{keyStr}&{valStr}";

            return Encrypt.Md5(txt);
        }

        /// <summary>
        /// 获取当前app的配置集合的md5版本，1分钟缓存 集合了继承app的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<string> AppPublishedConfigsMd5CacheWithInheritanced(string appId, string env)
        {
            var cacheKey = AppPublishedConfigsMd5CacheKeyWithInheritanced(appId, env);
            if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out string md5))
            {
                return md5;
            }

            md5 = await AppPublishedConfigsMd5WithInheritanced(appId, env);

            var cacheOp = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _memoryCache?.Set(cacheKey, md5, cacheOp);

            return md5;
        }

        public void Dispose()
        {
            _appService?.Dispose();
            _settingService?.Dispose();
            _userService?.Dispose();
        }

        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 发布当前待发布的配置项
        /// </summary>
        /// <param name="appId">应用id</param>
        /// <param name="ids">待发布的id列表</param>
        /// <param name="log">发布日志</param>
        /// <param name="operatorr">操作员</param>
        /// <returns></returns>
        public async Task<(bool result, string publishTimelineId)> Publish(string appId, string[] ids, string log, string operatorr, string env)
        {
            await _lock.WaitAsync();
            using var uow = _uowAccessor(env);
            try
            {
                using var configRepository = _configRepositoryAccessor(env);
                configRepository.Uow = uow;
                using var publishTimelineRepository = _publishTimelineRepositoryAccsssor(env);
                publishTimelineRepository.Uow = uow;
                using var configPublishedRepository = _configPublishedRepositoryAccessor(env);
                configPublishedRepository.Uow = uow;
                using var publishDetailRepository = _publishDetailRepositoryAccessor(env);
                publishDetailRepository.Uow = uow;

                uow?.Begin();

                var waitPublishConfigs = await configRepository.QueryAsync(x =>
                 x.AppId == appId &&
                 x.Env == env &&
                 x.Status == ConfigStatus.Enabled &&
                 x.EditStatus != EditStatus.Commit);

                if (ids != null && ids.Any())
                {
                    //如果ids传值了，过滤一下
                    waitPublishConfigs = waitPublishConfigs.Where(x => ids.Contains(x.Id)).ToList();
                }
                //这里默认admin console 实例只部署一个，如果部署多个同步操作，高并发的时候这个version会有问题
                var publishList = await publishTimelineRepository.QueryAsync(x => x.AppId == appId);
                var versionMax = publishList.Any() ? publishList.Max(x => x.Version) : 0;

                var user = await _userService.GetUserAsync(operatorr);

                var publishTimelineNode = new PublishTimeline();
                publishTimelineNode.AppId = appId;
                publishTimelineNode.Id = Guid.NewGuid().ToString("N");
                publishTimelineNode.PublishTime = DateTime.Now;
                publishTimelineNode.PublishUserId = user?.Id;
                publishTimelineNode.PublishUserName = user?.UserName;
                publishTimelineNode.Version = versionMax + 1;
                publishTimelineNode.Log = log;
                publishTimelineNode.Env = env;

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
                        Version = publishTimelineNode.Version,
                        Env = env
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
                var publishedConfigs = await configPublishedRepository
                    .QueryAsync(x => x.Status == ConfigStatus.Enabled && x.AppId == appId && x.Env == env);
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
                        Value = x.Value,
                        Env = x.Env
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
                            Version = publishTimelineNode.Version,
                            Env = env
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

                await configRepository.UpdateAsync(waitPublishConfigs);
                await publishTimelineRepository.InsertAsync(publishTimelineNode);
                await publishDetailRepository.InsertAsync(publishDetails);
                await configPublishedRepository.UpdateAsync(publishedConfigs);
                await configPublishedRepository.InsertAsync(publishedConfigsCopy);

                await uow?.SaveChangesAsync();

                ClearAppPublishedConfigsMd5Cache(appId, env);
                ClearAppPublishedConfigsMd5CacheWithInheritanced(appId, env);

                return (true, publishTimelineNode.Id);
            }
            catch (Exception exc)
            {
                uow?.Rollback();
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> IsPublishedAsync(string configId, string env)
        {
            using var repository = _configPublishedRepositoryAccessor(env);
            var any = await repository.QueryAsync(
                x => x.ConfigId == configId
                     && x.Env == env
                     && x.Status == ConfigStatus.Enabled);

            return any.Count > 0;
        }

        public async Task<List<PublishDetail>> GetPublishDetailByPublishTimelineIdAsync(string publishTimelineId,
            string env)
        {
            using var repository = _publishDetailRepositoryAccessor(env);
            var list = await repository
                .QueryAsync(x => x.PublishTimelineId == publishTimelineId && x.Env == env);

            return list;
        }

        public async Task<PublishTimeline> GetPublishTimeLineNodeAsync(string publishTimelineId, string env)
        {
            using var repository = _publishTimelineRepositoryAccsssor(env);
            var one = (await repository.QueryAsync(x => x.Id == publishTimelineId && x.Env == env))
                .FirstOrDefault();

            return one;
        }

        public async Task<List<PublishTimeline>> GetPublishTimelineHistoryAsync(string appId, string env)
        {
            using var repository = _publishTimelineRepositoryAccsssor(env);
            var list = await repository.QueryAsync(x => x.AppId == appId && x.Env == env);

            return list;
        }

        public async Task<List<PublishDetail>> GetPublishDetailListAsync(string appId, string env)
        {
            using var repository = _publishDetailRepositoryAccessor(env);
            var list = await repository.QueryAsync(x => x.AppId == appId && x.Env == env);

            return list;
        }

        public async Task<List<PublishDetail>> GetConfigPublishedHistory(string configId, string env)
        {
            using var repository = _publishDetailRepositoryAccessor(env);
            var list = await repository.QueryAsync(x => x.ConfigId == configId && x.Env == env);

            return list;
        }

        public async Task<List<ConfigPublished>> GetPublishedConfigsAsync(string appId, string env)
        {
            using var repository = _configPublishedRepositoryAccessor(env);
            var list = await repository.QueryAsync(x => x.AppId == appId && x.Status == ConfigStatus.Enabled && x.Env == env);

            return list;
        }

        public async Task<ConfigPublished> GetPublishedConfigAsync(string configId, string env)
        {
            using var repository = _configPublishedRepositoryAccessor(env);
            var one = (await repository.QueryAsync(x => x.ConfigId == configId
                                                                 && x.Status == ConfigStatus.Enabled
                                                                 && x.Env == env
            )).FirstOrDefault();

            return one;
        }

        public async Task<bool> RollbackAsync(string publishTimelineId, string env)
        {
            using var uow = _uowAccessor(env);

            using var configRepository = _configRepositoryAccessor(env);
            using var publishTimelineRepository = _publishTimelineRepositoryAccsssor(env);
            using var configPublishedRepository = _configPublishedRepositoryAccessor(env);
            using var publishDetailRepository = _publishDetailRepositoryAccessor(env);

            configRepository.Uow = uow;
            publishTimelineRepository.Uow = uow;
            configPublishedRepository.Uow = uow;
            publishDetailRepository.Uow = uow;

            uow?.Begin();

            var publishNode = (await publishTimelineRepository.QueryAsync(x => x.Id == publishTimelineId && x.Env == env)).FirstOrDefault();

            var version = publishNode.Version;
            var appId = publishNode.AppId;

            var latest = (await publishTimelineRepository.QueryAsync(x => x.AppId == appId && x.Env == env))
                .OrderByDescending(x => x.Version).FirstOrDefault();

            if (latest.Id == publishTimelineId)
            {
                //当前版本直接返回true
                return true;
            }

            var publishedConfigs = await configPublishedRepository
                .QueryAsync(x => x.AppId == appId && x.Version == version && x.Env == env);
            var currentConfigs = await configRepository
                .QueryAsync(x => x.AppId == appId && x.Status == ConfigStatus.Enabled && x.Env == env);

            //把当前的全部软删除
            foreach (var item in currentConfigs)
            {
                item.Status = ConfigStatus.Deleted;
            }

            await configRepository.UpdateAsync(currentConfigs);
            //根据id把所有发布项目设置为启用
            var now = DateTime.Now;
            foreach (var item in publishedConfigs)
            {
                var config = (await configRepository.QueryAsync(x => x.AppId == appId && x.Id == item.ConfigId)).FirstOrDefault();
                config.Status = ConfigStatus.Enabled;
                config.Value = item.Value;
                config.UpdateTime = now;
                config.EditStatus = EditStatus.Commit;
                config.OnlineStatus = OnlineStatus.Online;

                await configRepository.UpdateAsync(config);
            }

            //删除version之后的版本
            var configPublishedConfigs = await configPublishedRepository.QueryAsync(x => x.AppId == appId && x.Env == env && x.Version > version);
            await configPublishedRepository.DeleteAsync(configPublishedConfigs);
            //设置为发布状态
            foreach (var item in publishedConfigs)
            {
                item.Status = ConfigStatus.Enabled;
                await configPublishedRepository.UpdateAsync(item);
            }

            //删除发布时间轴version之后的版本
            var deletePublishTimeLineItems = await publishTimelineRepository.QueryAsync(x => x.AppId == appId && x.Env == env && x.Version > version);
            await publishTimelineRepository.DeleteAsync(deletePublishTimeLineItems);
            var deletePublishDetailItems = await publishDetailRepository.QueryAsync(x => x.AppId == appId && x.Env == env && x.Version > version);
            await publishDetailRepository.DeleteAsync(deletePublishDetailItems);

            await uow?.SaveChangesAsync();

            ClearAppPublishedConfigsMd5Cache(appId, env);
            ClearAppPublishedConfigsMd5CacheWithInheritanced(appId, env);

            return true;
        }

        public async Task<bool> EnvSync(string appId, string currentEnv, List<string> toEnvs)
        {
            var currentEnvConfigs = await this.GetByAppIdAsync(appId, currentEnv);

            foreach (var env in toEnvs)
            {
                var envConfigs = await this.GetByAppIdAsync(appId, env);
                var addRanges = new List<Config>();
                var updateRanges = new List<Config>();
                foreach (var currentEnvConfig in currentEnvConfigs)
                {
                    var envConfig = envConfigs.FirstOrDefault(x => GenerateKey(x) == GenerateKey(currentEnvConfig));
                    if (envConfig == null)
                    {
                        //没有相同的配置，则添加
                        currentEnvConfig.Id = Guid.NewGuid().ToString("N");
                        currentEnvConfig.Env = env;
                        currentEnvConfig.CreateTime = DateTime.Now;
                        currentEnvConfig.UpdateTime = DateTime.Now;
                        currentEnvConfig.Status = ConfigStatus.Enabled;
                        currentEnvConfig.EditStatus = EditStatus.Add;
                        currentEnvConfig.OnlineStatus = OnlineStatus.WaitPublish; //全部设置为待发布状态
                        addRanges.Add(currentEnvConfig);
                    }
                    else
                    {
                        // 如果有了相同的键，如果值不同，则更新
                        if (envConfig.Value != currentEnvConfig.Value)
                        {
                            envConfig.UpdateTime = DateTime.Now;
                            envConfig.Value = currentEnvConfig.Value;
                            if (envConfig.EditStatus == EditStatus.Commit)
                            {
                                envConfig.EditStatus = EditStatus.Edit;
                            }

                            envConfig.OnlineStatus = OnlineStatus.WaitPublish;
                            envConfig.Description = currentEnvConfig.Description;
                            updateRanges.Add(envConfig);
                        }
                    }
                }

                if (addRanges.Count > 0)
                {
                    await this.AddRangeAsync(addRanges, env);
                }

                if (updateRanges.Count > 0)
                {
                    await this.UpdateAsync(updateRanges, env);
                }
            }

            return true;
        }

        /// <summary>
        /// 获取配置项转换成键值对列表形式
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public async Task<List<KeyValuePair<string, string>>> GetKvListAsync(string appId, string env)
        {
            var configs = await GetByAppIdAsync(appId, env);
            var kvList = new List<KeyValuePair<string, string>>();
            foreach (var config in configs)
            {
                kvList.Add(new KeyValuePair<string, string>(GenerateKey(config), config.Value));
            }

            return kvList;
        }

        private async Task<bool> SaveFromDictAsync(IDictionary<string, string> dict, string appId, string env, bool isPatch)
        {
            using var uow = _uowAccessor(env);
            using var configRepository = _configRepositoryAccessor(env);

            configRepository.Uow = uow;

            uow?.Begin();

            var currentConfigs = await configRepository
                .QueryAsync(x => x.AppId == appId && x.Env == env && x.Status == ConfigStatus.Enabled);
            var addConfigs = new List<Config>();
            var updateConfigs = new List<Config>();
            var deleteConfigs = new List<Config>();

            var now = DateTime.Now;

            foreach (var kv in dict)
            {
                var key = kv.Key;
                var value = kv.Value;
                var config = currentConfigs.FirstOrDefault(x => GenerateKey(x) == key);
                if (config == null)
                {
                    var gk = SplitJsonKey(key);
                    addConfigs.Add(new Config
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        AppId = appId,
                        Env = env,
                        Key = gk.Item2,
                        Group = gk.Item1,
                        Value = value,
                        CreateTime = now,
                        Status = ConfigStatus.Enabled,
                        EditStatus = EditStatus.Add,
                        OnlineStatus = OnlineStatus.WaitPublish
                    });
                }
                else if (config.Value != kv.Value)
                {
                    config.Value = value;
                    config.UpdateTime = now;
                    if (config.OnlineStatus == OnlineStatus.Online)
                    {
                        config.EditStatus = EditStatus.Edit;
                        config.OnlineStatus = OnlineStatus.WaitPublish;
                    }
                    else
                    {
                        if (config.EditStatus == EditStatus.Add)
                        {
                            //do nothing
                        }

                        if (config.EditStatus == EditStatus.Edit)
                        {
                            //do nothing
                        }

                        if (config.EditStatus == EditStatus.Deleted)
                        {
                            //上一次是删除状态，现在恢复为编辑状态
                            config.EditStatus = EditStatus.Edit;
                        }
                    }

                    updateConfigs.Add(config);
                }
            }

            if (!isPatch)//补丁模式不删除现有配置,只有全量模式才删除
            {
                var keys = dict.Keys.ToList();
                foreach (var item in currentConfigs)
                {
                    var key = GenerateKey(item);
                    if (!keys.Contains(key))
                    {
                        item.EditStatus = EditStatus.Deleted;
                        item.OnlineStatus = OnlineStatus.WaitPublish;
                        deleteConfigs.Add(item);
                    }
                }
            }

            if (addConfigs.Any())
            {
                await configRepository.InsertAsync(addConfigs);
            }

            if (updateConfigs.Any())
            {
                await configRepository.UpdateAsync(updateConfigs);
            }

            if (deleteConfigs.Any())
            {
                await configRepository.UpdateAsync(deleteConfigs);
            }

            await uow?.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 保存json字符串为配置项
        /// </summary>
        /// <param name="json"></param>
        /// <param name="appId"></param>
        /// <param name="env"></param>
        /// <param name="isPatch"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> SaveJsonAsync(string json, string appId, string env, bool isPatch)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json");
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            using var stream = new MemoryStream(byteArray);
            var dict = JsonConfigurationFileParser.Parse(stream);

            return await SaveFromDictAsync(dict, appId, env, isPatch);
        }

        public (bool, string) ValidateKvString(string kvStr)
        {
            StringReader sr = new StringReader(kvStr);
            int row = 0;
            var dict = new Dictionary<string, string>();
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }

                row++;
                //必须要有=号
                if (line.IndexOf('=') < 0)
                {
                    return (false, $"第 {row} 行缺少等号。");
                }

                var index = line.IndexOf('=');
                var key = line.Substring(0, index);
                if (dict.ContainsKey(key))
                {
                    return (false, $"键 {key} 重复。");
                }

                dict.Add(key, "");
            }

            return (true, "");
        }

        public void ClearCache()
        {
            if (_memoryCache != null && _memoryCache is MemoryCache memCache)
            {
                memCache.Compact(1.0);
            }
        }

        public async Task<bool> SaveKvListAsync(string kvString, string appId, string env, bool isPatch)
        {
            if (kvString == null)
            {
                throw new ArgumentNullException(nameof(kvString));
            }

            StringReader sr = new StringReader(kvString);
            var dict = new Dictionary<string, string>();
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }

                var index = line.IndexOf('=');
                if (index < 0)
                {
                    continue;
                }

                var key = line.Substring(0, index);
                var val = line.Substring(index + 1, line.Length - index - 1);

                dict.Add(key, val);
            }

            return await SaveFromDictAsync(dict, appId, env, isPatch);
        }

        private (string, string) SplitJsonKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var index = key.LastIndexOf(':');
            if (index >= 0)
            {
                var group = key.Substring(0, index);
                var newkey = key.Substring(index + 1, key.Length - index - 1);

                return (group, newkey);
            }

            return ("", key);
        }
    }
}