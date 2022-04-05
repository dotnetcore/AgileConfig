using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace AgileConfig.Server.Service
{
    public class ServiceInfoService : IServiceInfoService
    {
        private readonly IMemoryCache _memoryCache;

        public ServiceInfoService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<ServiceInfo> GetByUniqueIdAsync(string id)
        {
            var entity = await FreeSQL.Instance.Select<ServiceInfo>().Where(x => x.Id == id).FirstAsync();

            return entity;
        }

        public async Task<ServiceInfo> GetByServiceIdAsync(string serviceId)
        {
            var entity = await FreeSQL.Instance.Select<ServiceInfo>().Where(x => x.ServiceId == serviceId).FirstAsync();

            return entity;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            var aff = await FreeSQL.Instance.Delete<ServiceInfo>().Where(x => x.Id == id)
                .ExecuteAffrowsAsync();

            return aff > 0;
        }

        public async Task<List<ServiceInfo>> GetAllServiceInfoAsync()
        {
            var services = await FreeSQL.Instance.Select<ServiceInfo>().Where(x => 1 == 1).ToListAsync();

            return services;
        }

        public async Task<List<ServiceInfo>> GetOnlineServiceInfoAsync()
        {
            var services = await FreeSQL.Instance.Select<ServiceInfo>().Where(x => x.Status == ServiceStatus.Healthy)
                .ToListAsync();

            return services;
        }

        public async Task<List<ServiceInfo>> GetOfflineServiceInfoAsync()
        {
            var services = await FreeSQL.Instance.Select<ServiceInfo>().Where(x => x.Status == ServiceStatus.Unhealthy)
                .ToListAsync();

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
                        metaDataStr = string.Join(",", metaData.OrderBy(x => x));
                    }
                }

                sb.Append(
                    $"{serviceInfo.ServiceId}&{serviceInfo.ServiceName}&{serviceInfo.Ip}&{serviceInfo.Port}&{(int)serviceInfo.Status}&{metaDataStr}&");
            }

            var txt = sb.ToString();
            return Encrypt.Md5(txt);
        }

        public void ClearCache()
        {
            if (_memoryCache != null && _memoryCache is MemoryCache memCache)
            {
                memCache.Compact(1.0);
            }
        }

        public async Task UpdateServiceStatus(ServiceInfo service, ServiceStatus status)
        {
            var id = service.Id;
            var oldStatus = service.Status;

            if (status == ServiceStatus.Unhealthy)
            {
                await FreeSQL.Instance.Update<ServiceInfo>()
                    .Set(x => x.Status, status)
                    .Where(x => x.Id == id)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                await FreeSQL.Instance.Update<ServiceInfo>()
                    .Set(x => x.Status, status)
                    .Set(x => x.LastHeartBeat, DateTime.Now)
                    .Where(x => x.Id == id)
                    .ExecuteAffrowsAsync();
            }

            if (oldStatus != status)
            {
                ClearCache();
                dynamic param = new ExpandoObject();
                param.ServiceId = service.ServiceId;
                param.ServiceName = service.ServiceName;
                param.UniqueId = service.Id;
                TinyEventBus.Instance.Fire(EventKeys.UPDATE_SERVICE_STATUS, param);
            }
        }

        public void Dispose()
        {
        }
    }
}