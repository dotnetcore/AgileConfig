using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
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
    private MongodbAccess<Function> _functionAccess => new(_connectionString);
    private MongodbAccess<RoleFunction> _roleFunctionAccess => new(_connectionString);

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
                Creator = SystemSettings.SuperAdminId
            });

        return true;
    }

    public void EnsureSystemRolePermissions()
    {
        EnsureSystemRoles();
    }

    private void EnsureSystemRoles()
    {
        // Super Admin gets all permissions
        var superAdminPermissions = Functions.GetAllPermissions();
        EnsureRole(SystemRoleConstants.SuperAdminId, "Super Administrator");
        EnsureRolePermissions(SystemRoleConstants.SuperAdminId, superAdminPermissions);

        // Administrator gets all permissions (same as SuperAdmin)
        var adminPermissions = GetAdminPermissions();
        EnsureRole(SystemRoleConstants.AdminId, "Administrator");
        EnsureRolePermissions(SystemRoleConstants.AdminId, adminPermissions);

        // Operator gets all App and Config related permissions
        var operatorPermissions = GetOperatorPermissions();
        EnsureRole(SystemRoleConstants.OperatorId, "Operator");
        EnsureRolePermissions(SystemRoleConstants.OperatorId, operatorPermissions);
    }

    private static List<string> GetAdminPermissions()
    {
        // Administrator has all permissions same as SuperAdmin
        return Functions.GetAllPermissions();
    }

    private static List<string> GetOperatorPermissions()
    {
        // Operator has all App and Config related permissions
        return new List<string>
        {
            // All Application permissions
            Functions.App_Read,
            Functions.App_Add,
            Functions.App_Edit,
            Functions.App_Delete,
            Functions.App_Auth,

            // All Configuration permissions
            Functions.Config_Read,
            Functions.Config_Add,
            Functions.Config_Edit,
            Functions.Config_Delete,
            Functions.Config_Publish,
            Functions.Config_Offline
        };
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

    private void EnsureRolePermissions(string roleId, List<string> functionCodes)
    {
        // Get all functions from database
        var allFunctions = _functionAccess.MongoQueryable.ToList();

        // Get existing role-function mappings
        var existingRoleFunctions = _roleFunctionAccess.MongoQueryable.Where(x => x.RoleId == roleId).ToList();

        // Find functions that need to be assigned to this role
        var functionsToAssign = new List<RoleFunction>();
        foreach (var functionCode in functionCodes)
        {
            var function = allFunctions.FirstOrDefault(f => f.Code == functionCode);
            if (function != null)
                // Check if this role-function mapping already exists
                if (!existingRoleFunctions.Any(rf => rf.FunctionId == function.Id))
                    functionsToAssign.Add(new RoleFunction
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        RoleId = roleId,
                        FunctionId = function.Id,
                        CreateTime = DateTime.Now
                    });
        }

        // Insert new role-function mappings
        if (functionsToAssign.Count > 0) _roleFunctionAccess.Collection.InsertMany(functionsToAssign);

        // Remove role-function mappings that are no longer needed
        var functionIdsToKeep = allFunctions
            .Where(f => functionCodes.Contains(f.Code))
            .Select(f => f.Id)
            .ToList();

        var roleFunctionsToRemove = existingRoleFunctions
            .Where(rf => !functionIdsToKeep.Contains(rf.FunctionId))
            .Select(rf => rf.Id)
            .ToList();

        if (roleFunctionsToRemove.Count > 0)
            _roleFunctionAccess.Collection.DeleteMany(rf => roleFunctionsToRemove.Contains(rf.Id));
    }
}