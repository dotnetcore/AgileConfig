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

        public const string SuperAdminId = "super_admin";
        public const string SuperAdminUserName = "admin";

        public const string DefaultEnvironment = "DEV,TEST,STAGING,PROD";
        public const string DefaultEnvironmentKey = "environment";
        public const string DefaultJwtSecretKey = "jwtsecret";

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

        public async Task<Setting> GetAsync(string id)
        {
            return await _settingRepository.GetAsync(id);
        }

        public async Task<List<Setting>> GetAllSettingsAsync()
        {
            return await _settingRepository.AllAsync();
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
            su.Id = SuperAdminId;
            su.Password = password;
            su.Salt = newSalt;
            su.Status = UserStatus.Normal;
            su.Team = "";
            su.CreateTime = DateTime.Now;
            su.UserName = SuperAdminUserName;
            await _userRepository.InsertAsync(su);

            var userRoles = new List<UserRole>();
            var ursa = new UserRole()
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = Role.SuperAdmin,
                UserId = SuperAdminId
            };
            userRoles.Add(ursa);
            var ura = new UserRole()
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = Role.Admin,
                UserId = SuperAdminId
            };
            userRoles.Add(ura);

            await _userRoleRepository.InsertAsync(userRoles);

            return true;
        }

        public async Task<bool> HasSuperAdmin()
        {
            var admin = await _userRepository.GetAsync(SuperAdminId);

            return admin != null;
        }

        public async Task<bool> InitDefaultEnvironment()
        {
            var env = await _settingRepository.GetAsync(DefaultEnvironmentKey);
            if (env == null)
            {
                var setting = new Setting
                {
                    Id = DefaultEnvironmentKey,
                    Value = DefaultEnvironment,
                    CreateTime = DateTime.Now
                };
                await _settingRepository.InsertAsync(setting);

                return true;
            }

            return true;
        }

        public void Dispose()
        {
            _settingRepository.Dispose();
            _userRepository.Dispose();
            _userRoleRepository.Dispose();
        }

        public async Task<string[]> GetEnvironmentList()
        {
            var env = await _settingRepository.GetAsync(DefaultEnvironmentKey);

            return env?.Value?.ToUpper().Split(',') ?? [];
        }

        /// <summary>
        /// 如果 配置文件或者环境变量没配置 JwtSetting:SecurityKey 则生成一个存库
        /// </summary>
        /// <returns></returns>
        public bool TryInitJwtSecret()
        {
            var jwtSecretFromConfig = Global.Config["JwtSetting:SecurityKey"];
            if (string.IsNullOrEmpty(jwtSecretFromConfig))
            {
                var jwtSecretSetting = _settingRepository.GetAsync(DefaultEnvironmentKey).Result;
                if (jwtSecretSetting == null)
                {
                    var setting = new Setting
                    {
                        Id = DefaultJwtSecretKey,
                        Value = GenreateJwtSecretKey(),
                        CreateTime = DateTime.Now
                    };

                    try
                    {
                        _ = _settingRepository.InsertAsync(setting).Result;
                        return true;
                    }
                    catch (Exception e)
                    {
                        //处理异常，防止多个实例第一次启动的时候，并发生成key值，发生异常，导致服务起不来
                        Console.WriteLine(e);
                    }

                    return false;
                }
            }
            return true;
        }

        public string GetJwtTokenSecret()
        {
            var jwtSecretSetting =  _settingRepository.GetAsync(DefaultEnvironmentKey).Result;
            return jwtSecretSetting?.Value;
        }

        /// <summary>
        /// 生成一个 jwt 加密的 key ，38位
        /// </summary>
        /// <returns></returns>
        private string GenreateJwtSecretKey()
        {
            var guid1 = Guid.NewGuid().ToString("n");
            var guid2 =  Guid.NewGuid().ToString("n");

            return guid1.Substring(0, 19) + guid2.Substring(0, 19);
        }
    }
}
