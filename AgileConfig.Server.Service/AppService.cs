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

        public AppService(ISqlContext context)
        {
            _dbContext = context as AgileConfigDbContext;
        }

        public async Task<bool> AddAsync(App app)
        {
            await _dbContext.Apps.AddAsync(app);
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<bool> DeleteAsync(App app)
        {
            app = _dbContext.Apps.Find(app.Id);
            if (app != null)
            {
                _dbContext.Apps.Remove(app);
            }
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<bool> DeleteAsync(string appId)
        {
            var app = _dbContext.Apps.Find(appId);
            if (app != null)
            {
                _dbContext.Apps.Remove(app);
            }
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public Task<App> GetAsync(string id)
        {
            return _dbContext.Apps.FindAsync(id).AsTask();
        }

        public Task<List<App>> GetAllAppsAsync()
        {
            return _dbContext.Apps.ToListAsync();
        }

        public async Task<bool> UpdateAsync(App app)
        {
            _dbContext.Update(app);
            var x = await _dbContext.SaveChangesAsync();

            return x > 0;
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
