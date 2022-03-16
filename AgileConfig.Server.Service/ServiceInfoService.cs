using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AgileConfig.Server.Service
{
    public class ServiceInfoService : IServiceInfoService
    {
        private readonly FreeSqlContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public ServiceInfoService(FreeSqlContext freeSql,
            IMemoryCache memoryCache
            )
        {
            _memoryCache = memoryCache;
            _dbContext = freeSql;
        }

        public async Task<List<ServiceInfo>> GetAllServiceInfoAsync()
        {
            var services = await _dbContext.ServiceInfo.Where(x => 1 == 1).ToListAsync();

            return services;
        }

        public async Task<List<ServiceInfo>> GetOnlineServiceInfoAsync()
        {
            var services = await _dbContext.ServiceInfo.Where(x => x.Alive == ServiceAlive.Online).ToListAsync();
            
            return services;

        }

        public async Task<List<ServiceInfo>> GetOfflineServiceInfoAsync()
        {
            var services = await _dbContext.ServiceInfo.Where(x => x.Alive == ServiceAlive.Offline).ToListAsync();
            
            return services;
        }

        public async Task<string> ServicesMD5Cache()
        {
            var cacheKey = $"ServiceInfoService_ServicesMD5Cache";
            if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out string md5))
            {
                return md5;
            }
            
            md5 = await ServicesMD5();
            var cacheOp = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _memoryCache?.Set(cacheKey, md5, cacheOp);

            return md5;
        }
        
        public async Task<string> ServicesMD5()
        {
            var services = await GetAllServiceInfoAsync();
            var md5 = GenerateMD5(services);
            
            return md5;
        }

        private string GenerateMD5(List<ServiceInfo> services)
        {
            var sb = new StringBuilder(); 
            foreach (var serviceInfo in services.OrderBy(x => x.ServiceId))
            {
                var metaDataStr = "";
                if (!string.IsNullOrEmpty(serviceInfo.MetaData))
                {
                    var metaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData);
                    if (metaData != null)
                    {
                        metaDataStr = string.Join(",", metaData.OrderBy(x=>x));
                    }
                }
                sb.Append($"{serviceInfo.ServiceId}&{serviceInfo.ServiceName}&{serviceInfo.Ip}&{serviceInfo.Port}&{(int)serviceInfo.Alive}&{metaDataStr}&");
            }

            var txt = sb.ToString();
            return Encrypt.Md5(txt);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}