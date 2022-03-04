using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Service
{
    public class SettingService : ISettingService
    {
        private FreeSqlContext _dbContext;

        public const string SuperAdminId = "super_admin";
        public const string SuperAdminUserName = "admin";

        public const string DefaultEnvironment = "DEV,TEST,STAGING,PROD";
        public const string DefaultEnvironmentKey = "environment";

        public SettingService(FreeSqlContext context)
        {
            _dbContext = context;
        }

        public async Task<bool> AddAsync(Setting setting)
        {
            await _dbContext.Settings.AddAsync(setting);
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<bool> DeleteAsync(Setting setting)
        {
            setting = await _dbContext.Settings.Where(s => s.Id == setting.Id).ToOneAsync();
            if (setting != null)
            {
                _dbContext.Settings.Remove(setting);
            }
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<bool> DeleteAsync(string settingId)
        {
            var setting = await _dbContext.Settings.Where(s => s.Id == settingId).ToOneAsync();
            if (setting != null)
            {
                _dbContext.Settings.Remove(setting);
            }
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<Setting> GetAsync(string id)
        {
            return await _dbContext.Settings.Where(s => s.Id == id).ToOneAsync();
        }

        public async Task<List<Setting>> GetAllSettingsAsync()
        {
            return await _dbContext.Settings.Where(s => 1 == 1).ToListAsync();
        }

        public async Task<bool> UpdateAsync(Setting setting)
        {
            _dbContext.Update(setting);
            var x = await _dbContext.SaveChangesAsync();

            return x > 0;
        }

        public async Task<bool> SetSuperAdminPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var newSalt = Guid.NewGuid().ToString("N");
            password = Encrypt.Md5((password + newSalt));

            var su = new User();
            su.Id = SuperAdminId;
            su.Password = password;
            su.Salt = newSalt;
            su.Status = UserStatus.Normal;
            su.Team = "";
            su.CreateTime = DateTime.Now;
            su.UserName = SuperAdminUserName;
            _dbContext.Users.Add(su);

            var ursa = new UserRole()
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = Role.SuperAdmin,
                UserId = SuperAdminId
            };
            _dbContext.UserRoles.Add(ursa);
            var ura = new UserRole()
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = Role.Admin,
                UserId = SuperAdminId
            };
            _dbContext.UserRoles.Add(ura);

            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> HasSuperAdmin()
        {
            var admin = await _dbContext.Users.Where(x => x.Id == SuperAdminId).FirstAsync();

            return admin != null;
        }

        public async Task<bool> InitDefaultEnvironment()
        {
            var env = await _dbContext.Settings.Where(x => x.Id == DefaultEnvironmentKey).FirstAsync();
            if (env == null)
            {
                _dbContext.Settings.Add(new Setting
                {
                    Id = DefaultEnvironmentKey,
                    Value = DefaultEnvironment,
                    CreateTime = DateTime.Now
                });
                var result = await _dbContext.SaveChangesAsync();

                return result > 0;
            }

            return true;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public async Task<string[]> GetEnvironmentList()
        {
            var env = await _dbContext.Settings.Where(x => x.Id == DefaultEnvironmentKey).FirstAsync();

            return env.Value.ToUpper().Split(',');
        }
    }
}
