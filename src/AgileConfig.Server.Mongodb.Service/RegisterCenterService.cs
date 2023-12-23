using System.Dynamic;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Mongodb.Service;

public class RegisterCenterService(
    IRepository<ServiceInfo> serviceInfoRepository,
    IServiceInfoService serviceInfoService,
    ILogger<RegisterCenterService> logger) : IRegisterCenterService
{
    public async Task<string> RegisterAsync(ServiceInfo serviceInfo)
    {
        if (serviceInfo == null)
        {
            throw new ArgumentNullException(nameof(serviceInfo));
        }


        //if exist
        var oldEntity = await serviceInfoRepository.SearchFor(x => x.ServiceId == serviceInfo.ServiceId)
            .FirstOrDefaultAsync();
        if (oldEntity != null)
        {
            oldEntity.RegisterTime = DateTime.Now;
            oldEntity.Status = ServiceStatus.Healthy;
            oldEntity.LastHeartBeat = DateTime.Now;
            oldEntity.ServiceName = serviceInfo.ServiceName;
            oldEntity.Ip = serviceInfo.Ip;
            oldEntity.Port = serviceInfo.Port;
            oldEntity.MetaData = serviceInfo.MetaData;
            oldEntity.HeartBeatMode = serviceInfo.HeartBeatMode;
            oldEntity.CheckUrl = serviceInfo.CheckUrl;
            oldEntity.AlarmUrl = serviceInfo.AlarmUrl;
            oldEntity.RegisterWay = serviceInfo.RegisterWay;
            await serviceInfoRepository.UpdateAsync(oldEntity);

            serviceInfoService.ClearCache();

            logger.LogInformation("registered service {ServiceId} {ServiceName} successful", serviceInfo.ServiceId,
                serviceInfo.ServiceName);

            return oldEntity.Id;
        }

        serviceInfo.RegisterTime = DateTime.Now;
        serviceInfo.LastHeartBeat = DateTime.Now;
        serviceInfo.Status = ServiceStatus.Healthy;
        serviceInfo.Id = Guid.NewGuid().ToString("n");

        await serviceInfoRepository.InsertAsync(serviceInfo);
        serviceInfoService.ClearCache();

        logger.LogInformation("registered service {ServiceId} {ServiceName} successful", serviceInfo.ServiceId,
            serviceInfo.ServiceName);

        return serviceInfo.Id;
    }

    public async Task<bool> UnRegisterAsync(string serviceUniqueId)
    {
        logger.LogInformation("try to unregister service {ServiceUniqueId}", serviceUniqueId);

        if (string.IsNullOrEmpty(serviceUniqueId))
        {
            throw new ArgumentNullException(nameof(serviceUniqueId));
        }

        var oldEntity = await serviceInfoRepository.SearchFor(x => x.Id == serviceUniqueId).FirstOrDefaultAsync();
        if (oldEntity == null)
        {
            //if not exist
            logger.LogInformation("not find the service {ServiceUniqueId} ", serviceUniqueId);
            return false;
        }
        
        await serviceInfoRepository.DeleteAsync(oldEntity.Id.ToString());

        serviceInfoService.ClearCache();

        logger.LogInformation("unregister service {ServiceId} {ServiceName} successful", oldEntity.ServiceId,
            oldEntity.ServiceName);

        return true;
    }

    public async Task<bool> UnRegisterByServiceIdAsync(string serviceId)
    {
        logger.LogInformation("try to unregister service {ServiceId}", serviceId);

        if (string.IsNullOrEmpty(serviceId))
        {
            throw new ArgumentNullException(nameof(serviceId));
        }

        var oldEntity = await serviceInfoRepository.SearchFor(x => x.ServiceId == serviceId).FirstOrDefaultAsync();
        if (oldEntity == null)
        {
            //if not exist
            logger.LogInformation("not find the service {ServiceId}", serviceId);
            return false;
        }
        
        await serviceInfoRepository.DeleteAsync(oldEntity.Id.ToString());

        serviceInfoService.ClearCache();

        logger.LogInformation("unregister service {ServiceId} {ServiceName} successful", oldEntity.ServiceId,
            oldEntity.ServiceName);

        return true;
    }

    public async Task<bool> ReceiveHeartbeatAsync(string serviceUniqueId)
    {
        var entity = await serviceInfoRepository.FindAsync(serviceUniqueId);
        if (entity == null)
        {
            return false;
        }

        logger.LogInformation("receive service {ServiceId} {ServiceName} heartbeat", entity.ServiceId, entity.ServiceName);

        if (entity.HeartBeatMode == "server")
        {
            //如果是server模式，则不作为服务是否在线的判断依据        
        }
        else
        {
            var oldStatus = entity.Status;
            entity.Status = ServiceStatus.Healthy;
            entity.LastHeartBeat = DateTime.Now;
            await serviceInfoRepository.UpdateAsync(entity);


            if (oldStatus != ServiceStatus.Healthy)
            {
                serviceInfoService.ClearCache();
                dynamic param = new ExpandoObject();
                param.ServiceId = entity.ServiceId;
                param.ServiceName = entity.ServiceName;
                param.UniqueId = entity.Id;
                TinyEventBus.Instance.Fire(EventKeys.UPDATE_SERVICE_STATUS, param);
            }
        }

        return true;
    }
}