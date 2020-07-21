using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IModifyLogService
    {
        Task<List<ModifyLog>> Search(string configId);

        Task<bool> AddAsync(ModifyLog Log);
        Task<bool> AddRangAsync(List<ModifyLog> Logs);

        Task<ModifyLog> GetAsync(string logId);
    }
}
