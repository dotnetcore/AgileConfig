using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.Service
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public SettingService(
            ISettingRepository settingRepository,
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository)
        {
            _settingRepository = settingRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<bool> AddAsync(Setting setting)
        {
            await _settingRepository.InsertAsync(setting);
            return true;
        }

        public async Task<bool> DeleteAsync(Setting setting)
        {
            var setting2 = await _settingRepository.GetAsync(setting.Id);
            if (setting2 != null)
            {
                await _settingRepository.DeleteAsync(setting);
            }
            return true;
        }

        public async Task<bool> DeleteAsync(string settingId)
        {
            var setting = await _settingRepository.GetAsync(settingId);
            if (setting != null)
            {
                await _settingRepository.DeleteAsync(setting);
            }
            return true;
        }

        public Task<Setting> GetAsync(string id)
        {
            return _settingRepository.GetAsync(id);
        }

        public Task<List<Setting>> GetAllSettingsAsync()
        {
            return _settingRepository.AllAsync();
        }

        public async Task<bool> UpdateAsync(Setting setting)
        {
            await _settingRepository.UpdateAsync(setting);
            return true;
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
            su.Id = SystemSettings.SuperAdminId;
            su.Password = password;
            su.Salt = newSalt;
            su.Status = UserStatus.Normal;
            su.Team = "";
            su.CreateTime = DateTime.Now;
            su.UserName = SystemSettings.SuperAdminUserName;
            await _userRepository.InsertAsync(su);

            var userRoles = new List<UserRole>();
            var ursa = new UserRole()
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = Role.SuperAdmin,
                UserId = SystemSettings.SuperAdminId
            };
            userRoles.Add(ursa);
            var ura = new UserRole()
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = Role.Admin,
                UserId = SystemSettings.SuperAdminId
            };
            userRoles.Add(ura);

            await _userRoleRepository.InsertAsync(userRoles);

            return true;
        }

        public async Task<bool> HasSuperAdmin()
        {
            var admin = await _userRepository.GetAsync(SystemSettings.SuperAdminId);

            return admin != null;
        }

        public void Dispose()
        {
            _settingRepository.Dispose();
            _userRepository.Dispose();
            _userRoleRepository.Dispose();
        }

        public async Task<string[]> GetEnvironmentList()
        {
            var env = await _settingRepository.GetAsync(SystemSettings.DefaultEnvironmentKey);

            return env?.Value?.ToUpper().Split(',') ?? [];
        }
    }
}
