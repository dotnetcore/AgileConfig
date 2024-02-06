using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AgileConfig.Server.Service
{
    public class ServiceInfoService : IServiceInfoService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceInfoRepository _serviceInfoRepository;

        public ServiceInfoService(IMemoryCache memoryCache, IServiceInfoRepository serviceInfoRepository)
        {
            _memoryCache = memoryCache;
            _serviceInfoRepository = serviceInfoRepository;
        }

        public Task<ServiceInfo> GetByUniqueIdAsync(string id)
        {
            return _serviceInfoRepository.GetAsync(id);
        }

        public async Task<ServiceInfo> GetByServiceIdAsync(string serviceId)
        {
            var entity = (await _serviceInfoRepository.QueryAsync(x => x.ServiceId == serviceId)).FirstOrDefault();

            return entity;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            await _serviceInfoRepository.DeleteAsync(id);
            return true;
        }

        public Task<List<ServiceInfo>> GetAllServiceInfoAsync()
        {
            return _serviceInfoRepository.AllAsync();
        }

        public Task<List<ServiceInfo>> GetOnlineServiceInfoAsync()
        {
            return _serviceInfoRepository.QueryAsync(x => x.Status == ServiceStatus.Healthy);
        }

        public Task<List<ServiceInfo>> GetOfflineServiceInfoAsync()
        {
            return _serviceInfoRepository.QueryAsync(x => x.Status == ServiceStatus.Unhealthy);
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
            foreach (var serviceInfo in services.OrderBy(x => x.ServiceId, StringComparer.Ordinal))
            {
                var metaDataStr = "";
                if (!string.IsNullOrEmpty(serviceInfo.MetaData))
                {
                    var metaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData);
                    if (metaData != null)
                    {
                        metaDataStr = string.Join(",", metaData.OrderBy(x => x, StringComparer.Ordinal));
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

            var service2 = await _serviceInfoRepository.GetAsync(id);
            if (service2 == null) return;

            service2.Status = status;
            if (status != ServiceStatus.Unhealthy)
            {
                service2.LastHeartBeat = DateTime.Now;
            }
            await _serviceInfoRepository.UpdateAsync(service2);

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
            _serviceInfoRepository.Dispose();
        }

        public Task<List<ServiceInfo>> QueryAsync(Expression<Func<ServiceInfo, bool>> exp)
        {
            return _serviceInfoRepository.QueryAsync(exp);
        }
    }
}