using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;

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
        if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

        var newSalt = Guid.NewGuid().ToString("N");
        password = Encrypt.Md5(password + newSalt);

        var sql = freeSqlFactory.Create();

        EnsureSystemRoles(sql);

        var user = new User();
        user.Id = SystemSettings.SuperAdminId;
        user.Password = password;
        user.Salt = newSalt;
        user.Status = UserStatus.Normal;
        user.Team = "";
        user.CreateTime = DateTime.Now;
        user.UserName = SystemSettings.SuperAdminUserName;

        sql.Insert(user).ExecuteAffrows();

        var now = DateTime.Now;
        var userRoles = new List<UserRole>();
        userRoles.Add(new UserRole
        {
            Id = Guid.NewGuid().ToString("N"),
            RoleId = SystemRoleConstants.SuperAdminId,
            UserId = SystemSettings.SuperAdminId,
            CreateTime = now
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
        if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException(nameof(appName));

        var sql = freeSqlFactory.Create();
        var anyDefaultApp = sql.Select<App>().Any(x => x.Id == appName);
        ;
        if (!anyDefaultApp)
            sql.Insert(new App
            {
                Id = appName,
                Name = appName,
                Group = "",
                Secret = "",
                CreateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.PRIVATE,
                Creator = SystemSettings.SuperAdminId
            }).ExecuteAffrows();

        return true;
    }

    private static void EnsureSystemRoles(IFreeSql sql)
    {
        // Super Admin gets all permissions
        var superAdminPermissions = Functions.GetAllPermissions();
        EnsureRole(sql, SystemRoleConstants.SuperAdminId, "Super Administrator", superAdminPermissions);
        EnsureRolePermissions(sql, SystemRoleConstants.SuperAdminId, superAdminPermissions);

        // Administrator gets all permissions (same as SuperAdmin)
        var adminPermissions = GetAdminPermissions();
        EnsureRole(sql, SystemRoleConstants.AdminId, "Administrator", adminPermissions);
        EnsureRolePermissions(sql, SystemRoleConstants.AdminId, adminPermissions);

        // Operator gets all App and Config related permissions
        var operatorPermissions = GetOperatorPermissions();
        EnsureRole(sql, SystemRoleConstants.OperatorId, "Operator", operatorPermissions);
        EnsureRolePermissions(sql, SystemRoleConstants.OperatorId, operatorPermissions);
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

    private static void EnsureRole(IFreeSql sql, string id, string name, List<string> functions)
    {
        var role = sql.Select<Role>().Where(x => x.Id == id).First();

        if (role == null)
        {
            sql.Insert(new Role
            {
                Id = id,
                Name = name,
                Description = name,
                IsSystem = true,
                CreateTime = DateTime.Now
            }).ExecuteAffrows();
        }
        else
        {
            role.Name = name;
            role.Description = name;
            role.IsSystem = true;
            role.UpdateTime = DateTime.Now;
            sql.Update<Role>().SetSource(role).ExecuteAffrows();
        }
    }

    private static void EnsureRolePermissions(IFreeSql sql, string roleId, List<string> functionCodes)
    {
        // Get all functions from database
        var allFunctions = sql.Select<Function>().ToList();

        // Get existing role-function mappings
        var existingRoleFunctions = sql.Select<RoleFunction>().Where(x => x.RoleId == roleId).ToList();

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
        if (functionsToAssign.Count > 0) sql.Insert(functionsToAssign).ExecuteAffrows();

        // Remove role-function mappings that are no longer needed
        var functionIdsToKeep = allFunctions
            .Where(f => functionCodes.Contains(f.Code))
            .Select(f => f.Id)
            .ToList();

        var roleFunctionsToRemove = existingRoleFunctions
            .Where(rf => !functionIdsToKeep.Contains(rf.FunctionId))
            .ToList();

        if (roleFunctionsToRemove.Count > 0)
            sql.Delete<RoleFunction>()
                .Where(rf => roleFunctionsToRemove.Select(r => r.Id).Contains(rf.Id))
                .ExecuteAffrows();
    }
}
