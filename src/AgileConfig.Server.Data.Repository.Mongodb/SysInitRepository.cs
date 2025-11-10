using AgileConfig.Server.Common;
using MongoDB.Driver;

namespace AgileConfig.Server.Data.Repository.Mongodb;

public class SysInitRepository : ISysInitRepository
{
    private readonly IConfiguration _configuration;

    private readonly string _connectionString = "";

    public SysInitRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration["db:conn"] ?? "";
    }

    private MongodbAccess<Setting> _settingAccess => new(_connectionString);
    private MongodbAccess<User> _userAccess => new(_connectionString);
    private MongodbAccess<UserRole> _userRoleAccess => new(_connectionString);
    private MongodbAccess<Role> _roleAccess => new(_connectionString);
    private MongodbAccess<App> _appAccess => new(_connectionString);

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
        if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

        var newSalt = Guid.NewGuid().ToString("N");
        password = Encrypt.Md5(password + newSalt);

        EnsureSystemRoles();

        var user = new User();
        user.Id = SystemSettings.SuperAdminId;
        user.Password = password;
        user.Salt = newSalt;
        user.Status = UserStatus.Normal;
        user.Team = "";
        user.CreateTime = DateTime.Now;
        user.UserName = SystemSettings.SuperAdminUserName;

        _userAccess.Collection.InsertOne(user);

        var now = DateTime.Now;
        var userRoles = new List<UserRole>();
        userRoles.Add(new UserRole
        {
            Id = Guid.NewGuid().ToString("N"),
            RoleId = SystemRoleConstants.SuperAdminId,
            UserId = SystemSettings.SuperAdminId,
            CreateTime = now
        });
        userRoles.Add(new UserRole
        {
            Id = Guid.NewGuid().ToString("N"),
            RoleId = SystemRoleConstants.AdminId,
            UserId = SystemSettings.SuperAdminId,
            CreateTime = now
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
        if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException(nameof(appName));

        var anyDefaultApp = _appAccess.MongoQueryable.FirstOrDefault(x => x.Id == appName);
        ;
        if (anyDefaultApp == null)
            _appAccess.Collection.InsertOne(new App
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

        return true;
    }

    private void EnsureSystemRoles()
    {
        EnsureRole(SystemRoleConstants.SuperAdminId, "Super Administrator");
        EnsureRole(SystemRoleConstants.AdminId, "Administrator");
        EnsureRole(SystemRoleConstants.OperatorId, "Operator");
    }

    private void EnsureRole(string id, string name)
    {
        var role = _roleAccess.MongoQueryable.FirstOrDefault(x => x.Id == id);
        if (role == null)
        {
            _roleAccess.Collection.InsertOne(new Role
            {
                Id = id,
                Name = name,
                Description = name,
                IsSystem = true,
                CreateTime = DateTime.Now
            });
        }
        else
        {
            role.Name = name;
            role.Description = name;
            role.IsSystem = true;
            role.UpdateTime = DateTime.Now;
            _roleAccess.Collection.ReplaceOne(x => x.Id == id, role, new ReplaceOptions { IsUpsert = true });
        }
    }
}