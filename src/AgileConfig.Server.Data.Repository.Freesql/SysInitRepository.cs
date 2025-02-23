using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class SysInitRepository : ISysInitRepository
{
    private readonly IFreeSqlFactory freeSqlFactory;

    public SysInitRepository(IFreeSqlFactory freeSqlFactory)
    {
        this.freeSqlFactory = freeSqlFactory;
    }

    public string? GetDefaultEnvironmentFromDb()
    {
        var setting = freeSqlFactory.Create().Select<Setting>().Where(x => x.Id == SystemSettings.DefaultEnvironmentKey)
                 .ToOne();

        return setting?.Value;
    }

    public string? GetJwtTokenSecret()
    {
        var setting = freeSqlFactory.Create().Select<Setting>().Where(x => x.Id == SystemSettings.DefaultJwtSecretKey)
            .ToOne();

        return setting?.Value;
    }

    public void SaveInitSetting(Setting setting)
    {
        freeSqlFactory.Create().Insert(setting).ExecuteAffrows();
    }

    public bool InitSa(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        var newSalt = Guid.NewGuid().ToString("N");
        password = Encrypt.Md5((password + newSalt));

        var sql = freeSqlFactory.Create();

        var user = new User();
        user.Id = SystemSettings.SuperAdminId;
        user.Password = password;
        user.Salt = newSalt;
        user.Status = UserStatus.Normal;
        user.Team = "";
        user.CreateTime = DateTime.Now;
        user.UserName = SystemSettings.SuperAdminUserName;

        sql.Insert(user).ExecuteAffrows();

        var userRoles = new List<UserRole>();
        userRoles.Add(new UserRole()
        {
            Id = Guid.NewGuid().ToString("N"),
            Role = Role.SuperAdmin,
            UserId = SystemSettings.SuperAdminId
        });
        userRoles.Add(new UserRole()
        {
            Id = Guid.NewGuid().ToString("N"),
            Role = Role.Admin,
            UserId = SystemSettings.SuperAdminId
        });

        sql.Insert(userRoles).ExecuteAffrows();

        return true;
    }

    public bool HasSa()
    {
        var anySa = freeSqlFactory.Create().Select<User>().Any(x => x.Id == SystemSettings.SuperAdminId);

        return anySa;
    }

    public bool InitDefaultApp(string appName)
    {
        if (string.IsNullOrEmpty(appName))
        {
            throw new ArgumentNullException(nameof(appName));
        }

        var sql = freeSqlFactory.Create();
        var anyDefaultApp = sql.Select<App>().Any(x => x.Id == appName);
        ;
        if (!anyDefaultApp)
        {
            sql.Insert(new App()
            {
                Id = appName,
                Name = appName,
                Group = "",
                Secret = "",
                CreateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.PRIVATE,
                AppAdmin = SystemSettings.SuperAdminId
            }).ExecuteAffrows();
        }

        return true;
    }
}