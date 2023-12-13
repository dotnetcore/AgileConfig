using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Mongodb.Service;

public class SettingService(
    IRepository<Setting> repository,
    IRepository<User> userRepository,
    IRepository<UserRole> userRoleRepository) : ISettingService
{
    public const string SuperAdminId = "super_admin";
    public const string SuperAdminUserName = "admin";

    public const string DefaultEnvironment = "DEV,TEST,STAGING,PROD";
    public const string DefaultEnvironmentKey = "environment";
    public const string DefaultJwtSecretKey = "jwtsecret";

    public async Task<bool> AddAsync(Setting setting)
    {
        await repository.InsertAsync(setting);
        return true;
    }

    public async Task<bool> DeleteAsync(Setting setting)
    {
        var result = await repository.DeleteAsync(setting.Id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteAsync(string settingId)
    {
        var result = await repository.DeleteAsync(settingId);
        return result.DeletedCount > 0;
    }

    public Task<Setting> GetAsync(string id)
    {
        return repository.SearchFor(s => s.Id == id).FirstOrDefaultAsync();
    }

    public Task<List<Setting>> GetAllSettingsAsync()
    {
        return repository.SearchFor(s => true).ToListAsync();
    }

    public async Task<bool> UpdateAsync(Setting setting)
    {
        var result = await repository.UpdateAsync(setting);
        return result.ModifiedCount > 0;
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
        await userRepository.InsertAsync(su);

        var ursa = new UserRole()
        {
            Id = Guid.NewGuid().ToString("N"),
            Role = Role.SuperAdmin,
            UserId = SuperAdminId
        };

        var ura = new UserRole()
        {
            Id = Guid.NewGuid().ToString("N"),
            Role = Role.Admin,
            UserId = SuperAdminId
        };
        await userRoleRepository.InsertAsync(ursa, ura);

        return true;
    }

    public async Task<bool> HasSuperAdmin()
    {
        var admin = await userRepository.SearchFor(x => x.Id == SuperAdminId).FirstOrDefaultAsync();

        return admin != null;
    }

    public async Task<bool> InitDefaultEnvironment()
    {
        var env = await repository.SearchFor(x => x.Id == DefaultEnvironmentKey).FirstOrDefaultAsync();
        if (env == null)
        {
            await repository.InsertAsync(new Setting
            {
                Id = DefaultEnvironmentKey,
                Value = DefaultEnvironment,
                CreateTime = DateTime.Now
            });
        }

        return true;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<string[]> GetEnvironmentList()
    {
        var env = await repository.SearchFor(x => x.Id == DefaultEnvironmentKey).FirstAsync();

        return env.Value.ToUpper().Split(',');
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
            var jwtSecretSetting = repository.Find(DefaultJwtSecretKey);
            if (jwtSecretSetting == null)
            {
                try
                {
                    repository.Insert(new Setting
                    {
                        Id = DefaultJwtSecretKey,
                        Value = GenreateJwtSecretKey(),
                        CreateTime = DateTime.Now
                    });
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

    public string? GetJwtTokenSecret()
    {
        var jwtSecretSetting = repository.Find(DefaultJwtSecretKey);;
        return jwtSecretSetting?.Value;
    }

    /// <summary>
    /// 生成一个 jwt 加密的 key ，38位
    /// </summary>
    /// <returns></returns>
    private string GenreateJwtSecretKey()
    {
        var guid1 = Guid.NewGuid().ToString("n");
        var guid2 = Guid.NewGuid().ToString("n");

        return guid1.Substring(0, 19) + guid2.Substring(0, 19);
    }
}