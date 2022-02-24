using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service
{
    public class RegisterCenterService : IRegisterCenterService
    {
        private FreeSqlContext _dbContext;
        private ILogger<RegisterCenterService> _logger;
        public RegisterCenterService(FreeSqlContext freeSql, ILogger<RegisterCenterService> logger)
        {
            _dbContext = freeSql;
            _logger = logger;
        }
        public async Task<string> RegisterAsync(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                throw new ArgumentNullException(nameof(serviceInfo));
            }

            _logger.LogInformation("try to register service {0} {1}", serviceInfo.ServiceId, serviceInfo.ServiceName);
            
            //if exist
            var oldEntity = await _dbContext.ServiceInfo.Where(x=>x.ServiceId == serviceInfo.ServiceId).FirstAsync();
            if (oldEntity != null)
            {
                oldEntity.RegisterTime = DateTime.Now;
                oldEntity.Alive = ServiceAlive.Online;
                oldEntity.LastHeartBeat = DateTime.Now;
                oldEntity.ServiceName = serviceInfo.ServiceName;
                oldEntity.Ip = serviceInfo.Ip;
                oldEntity.Port = serviceInfo.Port;
                oldEntity.MetaData = serviceInfo.MetaData;
                oldEntity.HeartBeatMode = serviceInfo.HeartBeatMode;
                oldEntity.CheckUrl = serviceInfo.CheckUrl;
                await _dbContext.ServiceInfo.UpdateAsync(oldEntity);
                var rows = await _dbContext.SaveChangesAsync();

                _logger.LogInformation("registered service {0} {1} successful .", serviceInfo.ServiceId, serviceInfo.ServiceName);
                
                return oldEntity.Id;
            }

            serviceInfo.RegisterTime = DateTime.Now;
            serviceInfo.LastHeartBeat = DateTime.Now;
            serviceInfo.Alive = ServiceAlive.Online;
            serviceInfo.Id = Guid.NewGuid().ToString("n");

            _dbContext.ServiceInfo.Add(serviceInfo);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("registered service {0} {1} successful .", serviceInfo.ServiceId, serviceInfo.ServiceName);
            
            return serviceInfo.Id;
        }

        public async Task<bool> UnRegisterAsync(string serviceUniqueId)
        {
            _logger.LogInformation("try to unregister service {0}", serviceUniqueId);

            if (string.IsNullOrEmpty(serviceUniqueId))
            {
                throw new ArgumentNullException(nameof(serviceUniqueId));
            }

            var oldEntity = await _dbContext.ServiceInfo.Where(x => x.Id == serviceUniqueId).FirstAsync();
            if(oldEntity == null)
            {
                //if not exist
                _logger.LogInformation("not find the service {0} .", serviceUniqueId);
                return false;
            }

            _dbContext.ServiceInfo.Remove(oldEntity);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("unregister service {0} {1} successful .", oldEntity.ServiceId, oldEntity.ServiceName);

            return true;
        }

        public async Task<bool> ReceiveHeartbeatAsync(string serviceUniqueId)
        {
            var entity = await _dbContext.ServiceInfo.Where(x => x.Id == serviceUniqueId).FirstAsync();
            if (entity == null)
            {
                return false;
            }
            _logger.LogInformation("receive service {0} {1} heartbeat .", entity.ServiceId, entity.ServiceName);

            entity.Alive = ServiceAlive.Online;
            entity.LastHeartBeat = DateTime.Now;
            await _dbContext.UpdateAsync(entity);

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
