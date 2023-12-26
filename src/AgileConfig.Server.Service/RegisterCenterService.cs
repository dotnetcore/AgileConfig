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
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service
{
    public class RegisterCenterService : IRegisterCenterService
    {
        private readonly IServiceInfoRepository _serviceInfoRepository;
        private readonly ILogger<RegisterCenterService> _logger;
        private readonly IServiceInfoService _serviceInfoService;

        public RegisterCenterService(
            IServiceInfoRepository serviceInfoRepository,
            IServiceInfoService serviceInfoService,
            ILogger<RegisterCenterService> logger)
        {
            _serviceInfoRepository = serviceInfoRepository;
            _logger = logger;
            _serviceInfoService = serviceInfoService;
        }

        public async Task<string> RegisterAsync(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                throw new ArgumentNullException(nameof(serviceInfo));
            }

            _logger.LogInformation("try to register service {0} {1}", serviceInfo.ServiceId, serviceInfo.ServiceName);

            //if exist
            var oldEntity = (await _serviceInfoRepository.QueryAsync(x => x.ServiceId == serviceInfo.ServiceId)).FirstOrDefault();
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
                await _serviceInfoRepository.UpdateAsync(oldEntity);

                _serviceInfoService.ClearCache();

                _logger.LogInformation("registered service {0} {1} successful .", serviceInfo.ServiceId,
                    serviceInfo.ServiceName);

                return oldEntity.Id;
            }

            serviceInfo.RegisterTime = DateTime.Now;
            serviceInfo.LastHeartBeat = DateTime.Now;
            serviceInfo.Status = ServiceStatus.Healthy;
            serviceInfo.Id = Guid.NewGuid().ToString("n");

            await _serviceInfoRepository.InsertAsync(serviceInfo);

            _serviceInfoService.ClearCache();

            _logger.LogInformation("registered service {0} {1} successful .", serviceInfo.ServiceId,
                serviceInfo.ServiceName);

            return serviceInfo.Id;
        }

        public async Task<bool> UnRegisterAsync(string serviceUniqueId)
        {
            _logger.LogInformation("try to unregister service {0}", serviceUniqueId);

            if (string.IsNullOrEmpty(serviceUniqueId))
            {
                throw new ArgumentNullException(nameof(serviceUniqueId));
            }

            var oldEntity = await _serviceInfoRepository.GetAsync(serviceUniqueId);
            if (oldEntity == null)
            {
                //if not exist
                _logger.LogInformation("not find the service {0} .", serviceUniqueId);
                return false;
            }

            await _serviceInfoRepository.DeleteAsync(oldEntity);

            _serviceInfoService.ClearCache();

            _logger.LogInformation("unregister service {0} {1} successful .", oldEntity.ServiceId,
                oldEntity.ServiceName);

            return true;
        }

        public async Task<bool> UnRegisterByServiceIdAsync(string serviceId)
        {
            _logger.LogInformation("try to unregister service {0}", serviceId);

            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ArgumentNullException(nameof(serviceId));
            }

            var oldEntity = (await _serviceInfoRepository.QueryAsync(x => x.ServiceId == serviceId)).FirstOrDefault();
            if (oldEntity == null)
            {
                //if not exist
                _logger.LogInformation("not find the service {0} .", serviceId);
                return false;
            }

            await _serviceInfoRepository.DeleteAsync(oldEntity);

            _serviceInfoService.ClearCache();

            _logger.LogInformation("unregister service {0} {1} successful .", oldEntity.ServiceId,
                oldEntity.ServiceName);

            return true;
        }

        public async Task<bool> ReceiveHeartbeatAsync(string serviceUniqueId)
        {
            var entity = await _serviceInfoRepository.GetAsync(serviceUniqueId);
            if (entity == null)
            {
                return false;
            }

            _logger.LogInformation("receive service {0} {1} heartbeat .", entity.ServiceId, entity.ServiceName);

            if (entity.HeartBeatMode == "server")
            {
                //如果是server模式，则不作为服务是否在线的判断依据        
            }
            else
            {
                var oldStatus = entity.Status;
                entity.Status = ServiceStatus.Healthy;
                entity.LastHeartBeat = DateTime.Now;

                await _serviceInfoRepository.UpdateAsync(entity);

                if (oldStatus != ServiceStatus.Healthy)
                {
                    _serviceInfoService.ClearCache();
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
}