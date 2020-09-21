using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Service
{
    public class AppService : IAppService
    {
        private FreeSqlContext _dbContext;

        public AppService(FreeSqlContext context, ISysLogService sysLogService)
        {
            _dbContext = context;
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
            app = await _dbContext.Apps.Where(a => a.Id == app.Id).ToOneAsync();
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
            var app = await _dbContext.Apps.Where(a => a.Id == appId).ToOneAsync();
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
            return await _dbContext.Apps.Where(a => a.Id == id).ToOneAsync();
        }

        public async Task<List<App>> GetAllAppsAsync()
        {
            return await  _dbContext.Apps.Where(a => 1 == 1).ToListAsync();
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
            var q = await _dbContext.Apps.Where(a => a.Enabled == true).CountAsync();

            return (int)q;
        }

    }
}
