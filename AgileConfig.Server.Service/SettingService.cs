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
    public class SettingService : ISettingService
    {
        private AgileConfigDbContext _dbContext;

        public string AdminPasswordSettingKey => "AdminPassword";
        string AdminPasswordHashSaltKey => "AdminPasswordHashSalt";

        public SettingService(ISqlContext context)
        {
            _dbContext = context as AgileConfigDbContext;
        }

        public async Task<bool> AddAsync(Setting setting)
        {
            await _dbContext.Settings.AddAsync(setting);
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<bool> DeleteAsync(Setting setting)
        {
            setting = _dbContext.Settings.Find(setting.Id);
            if (setting != null)
            {
                _dbContext.Settings.Remove(setting);
            }
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<bool> DeleteAsync(string settingId)
        {
            var setting = _dbContext.Settings.Find(settingId);
            if (setting != null)
            {
                _dbContext.Settings.Remove(setting);
            }
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<Setting> GetAsync(string id)
        {
            return await _dbContext.Settings.FindAsync(id);
        }

        public async Task<List<Setting>> GetAllSettingsAsync()
        {
            return await _dbContext.Settings.ToListAsync();
        }

        public async Task<bool> UpdateAsync(Setting setting)
        {
            _dbContext.Update(setting);
            var x = await _dbContext.SaveChangesAsync();

            return x > 0;
        }

        public async Task<bool> SetAdminPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var newSalt = Guid.NewGuid().ToString("N");
            password = Encrypt.Md5((password + newSalt));
            await AddAsync(new Setting { Id = AdminPasswordHashSaltKey, Value = newSalt, CreateTime = DateTime.Now });
            return await AddAsync(new Setting { Id = AdminPasswordSettingKey, Value = password, CreateTime = DateTime.Now });
        }

        public async Task<bool> HasAdminPassword()
        {
            var password = await GetAsync(AdminPasswordSettingKey);

            return password != null;
        }

        public async Task<bool> ValidateAdminPassword(string password)
        {
            var dbPassword = await GetAsync(AdminPasswordSettingKey);
            if (dbPassword == null || string.IsNullOrEmpty(dbPassword.Value) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var salt = await GetAsync(AdminPasswordHashSaltKey);
            password = Encrypt.Md5((password + salt));

            return password == dbPassword.Value;
        }
    }
}
