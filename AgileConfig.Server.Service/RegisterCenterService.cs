using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class RegisterCenterService : IRegisterCenterService
    {
        private FreeSqlContext _dbContext;
        public RegisterCenterService(FreeSqlContext freeSql)
        {
            _dbContext = freeSql;
        }
        public async Task<string> RegisterAsync(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                throw new ArgumentNullException(nameof(serviceInfo));
            }

            //if exist
            var oldEntity = await _dbContext.ServiceInfo.Where(x=>x.ServiceId == serviceInfo.ServiceId).FirstAsync();
            if (oldEntity != null)
            {
                oldEntity.RegisterTime = DateTime.Now;
                oldEntity.Alive = ServiceAlive.Online;

                _dbContext.ServiceInfo.Update(oldEntity);
                var rows = await _dbContext.SaveChangesAsync();

                return oldEntity.Id;
            }

            serviceInfo.RegisterTime = DateTime.Now;
            serviceInfo.Alive = ServiceAlive.Online;
            serviceInfo.Id = Guid.NewGuid().ToString("n");

            _dbContext.ServiceInfo.Add(serviceInfo);
            await _dbContext.SaveChangesAsync();

            return serviceInfo.Id;
        }

        public async Task<bool> UnRegisterAsync(string serviceUniqueId)
        {
            if (string.IsNullOrEmpty(serviceUniqueId))
            {
                throw new ArgumentNullException(nameof(serviceUniqueId));
            }

            //if exist
            var oldEntity = await _dbContext.ServiceInfo.Where(x => x.Id == serviceUniqueId).FirstAsync();
            if(oldEntity == null)
            {
                return false;
            }

            _dbContext.ServiceInfo.Remove(oldEntity);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
