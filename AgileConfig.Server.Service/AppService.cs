using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using AgileConfig.Server.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.EntityFrameworkCore;

namespace AgileConfig.Server.Service
{
    public class AppService : IAppService
    {
        private AgileConfigDbContext _dbContext;
        private ISysLogService _sysLogService;

        public AppService(ISqlContext context, ISysLogService sysLogService)
        {
            _dbContext = context as AgileConfigDbContext;
            _sysLogService = sysLogService;
        }

        public async Task<bool> AddAsync(App app)
        {
            await _dbContext.Apps.AddAsync(app);
            int x = await _dbContext.SaveChangesAsync();
            var result =  x > 0;

            return result;
        }

        public async Task<bool> DeleteAsync(App app)
        {
            app = _dbContext.Apps.Find(app.Id);
            if (app != null)
            {
                _dbContext.Apps.Remove(app);
            }
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        public async Task<bool> DeleteAsync(string appId)
        {
            var app = _dbContext.Apps.Find(appId);
            if (app != null)
            {
                _dbContext.Apps.Remove(app);
            }
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        public async Task<App> GetAsync(string id)
        {
            return await _dbContext.Apps.FindAsync(id);
        }

        public async Task<List<App>> GetAllAppsAsync()
        {
            return await _dbContext.Apps.ToListAsync();
        }

        public async Task<bool> UpdateAsync(App app)
        {
            _dbContext.Update(app);
            var x = await _dbContext.SaveChangesAsync();

            var result = x > 0;

            return result;
        }

        public async Task<int> CountEnabledAppsAsync()
        {
            var q = await _dbContext.Apps.CountAsync(a => a.Enabled == true);

            return q;
        }

    }
}
