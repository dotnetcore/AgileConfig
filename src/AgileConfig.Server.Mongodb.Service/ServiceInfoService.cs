using System.Dynamic;
using System.Text;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;

namespace AgileConfig.Server.Mongodb.Service;

public class ServiceInfoService(IRepository<ServiceInfo> repository, IMemoryCache? memoryCache) : IServiceInfoService
{
    public async Task<ServiceInfo?> GetByUniqueIdAsync(string id)
    {
        var entity = await repository.FindAsync(id);
        return entity;
    }

    public async Task<ServiceInfo> GetByServiceIdAsync(string serviceId)
    {
        var entity = await repository.SearchFor(x => x.ServiceId == serviceId).FirstOrDefaultAsync();
        return entity;
    }

    public async Task<bool> RemoveAsync(string id)
    {
        var result = await repository.DeleteAsync(id);
        return result.DeletedCount > 0;
    }

    public async Task<List<ServiceInfo>> GetAllServiceInfoAsync()
    {
        var services = await repository.SearchFor(x => true).ToListAsync();

        return services;
    }

    public async Task<List<ServiceInfo>> GetOnlineServiceInfoAsync()
    {
        var services = await repository.SearchFor(x => x.Status == ServiceStatus.Healthy)
            .ToListAsync();

        return services;
    }

    public async Task<List<ServiceInfo>> GetOfflineServiceInfoAsync()
    {
        var services = await repository.SearchFor(x => x.Status == ServiceStatus.Unhealthy)
            .ToListAsync();

        return services;
    }

    public async Task<string?> ServicesMD5Cache()
    {
        var cacheKey = $"ServiceInfoService_ServicesMD5Cache";
        if (memoryCache != null && memoryCache.TryGetValue(cacheKey, out string? md5))
        {
            return md5;
        }

        md5 = await ServicesMD5();
        var cacheOp = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
        memoryCache?.Set(cacheKey, md5, cacheOp);

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
        if (memoryCache is MemoryCache memCache)
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
            var update = Builders<ServiceInfo>.Update.Set(x => x.Status, status);
            await repository.UpdateAsync(x => x.Id == id, update);
        }
        else
        {
            var update = Builders<ServiceInfo>.Update
                .Set(x => x.Status, status)
                .Set(x => x.LastHeartBeat,DateTime.Now);
            await repository.UpdateAsync(x => x.Id == id, update);
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
        GC.SuppressFinalize(this);
    }
}