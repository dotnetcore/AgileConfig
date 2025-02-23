using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SysInitRepository : ISysInitRepository
{
    public SysInitRepository(IConfiguration configuration)
    {
        this._configuration = configuration;
        _connectionString = _configuration["db:conn"] ?? "";
    }

    private string _connectionString = "";
    private MongodbAccess<Setting> _settingAccess => new MongodbAccess<Setting>(_connectionString);
    private MongodbAccess<User> _userAccess => new MongodbAccess<User>(_connectionString);
    private MongodbAccess<UserRole> _userRoleAccess => new MongodbAccess<UserRole>(_connectionString);
    private MongodbAccess<App> _appAccess => new MongodbAccess<App>(_connectionString);

    private readonly IConfiguration _configuration;

    public string? GetDefaultEnvironmentFromDb()
    {
        var setting = _settingAccess.MongoQueryable.FirstOrDefault(x => x.Id == SystemSettings.DefaultEnvironmentKey);
        var val = setting?.Value;

        return val;
    }

    public string? GetJwtTokenSecret()
    {
        var setting = _settingAccess.MongoQueryable.FirstOrDefault(x => x.Id == SystemSettings.DefaultJwtSecretKey);

        return setting?.Value;
    }

    public void SaveInitSetting(Setting setting)
    {
        _settingAccess.Collection.InsertOne(setting);
    }

    public bool InitSa(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        var newSalt = Guid.NewGuid().ToString("N");
        password = Encrypt.Md5((password + newSalt));

        var user = new User();
        user.Id = SystemSettings.SuperAdminId;
        user.Password = password;
        user.Salt = newSalt;
        user.Status = UserStatus.Normal;
        user.Team = "";
        user.CreateTime = DateTime.Now;
        user.UserName = SystemSettings.SuperAdminUserName;

        _userAccess.Collection.InsertOne(user);

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

        _userRoleAccess.Collection.InsertMany(userRoles);

        return true;
    }

    public bool HasSa()
    {
        var sa = _userAccess.MongoQueryable.FirstOrDefault(x => x.Id == SystemSettings.SuperAdminId);

        return sa != null;
    }

    public bool InitDefaultApp(string appName)
    {
        if (string.IsNullOrEmpty(appName))
        {
            throw new ArgumentNullException(nameof(appName));
        }

        var anyDefaultApp = _appAccess.MongoQueryable.FirstOrDefault(x => x.Id == appName);
        ;
        if (anyDefaultApp == null)
        {
            _appAccess.Collection.InsertOne(new App()
            {
                Id = appName,
                Name = appName,
                Group = "",
                Secret = "",
                CreateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.PRIVATE,
                AppAdmin = SystemSettings.SuperAdminId
            });
        }

        return true;
    }
}