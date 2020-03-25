using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using AgileConfig.Server.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AgileConfig.Server.Service
{
    public class ModifyLogService : IModifyLogService
    {
        private AgileConfigDbContext _dbContext;
        public ModifyLogService(ISqlContext context)
        {
            _dbContext = context as AgileConfigDbContext;
        }

        public async Task<bool> AddAsync(ModifyLog log)
        {
            _dbContext.ModifyLogs.Add(log);
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<List<ModifyLog>> Search(string configId)
        {
            var logs = await _dbContext.ModifyLogs.Where(m => m.ConfigId == configId).ToListAsync();

            return logs;
        }
    }
}
