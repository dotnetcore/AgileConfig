using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Text.Json;

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
      userRoles.Add(new UserRole()
     {
    Id = Guid.NewGuid().ToString("N"),
      RoleId = SystemRoleConstants.SuperAdminId,
            UserId = SystemSettings.SuperAdminId,
            CreateTime = now
        });
  userRoles.Add(new UserRole()
   {
     Id = Guid.NewGuid().ToString("N"),
RoleId = SystemRoleConstants.AdminId,
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

    private static void EnsureSystemRoles(IFreeSql sql)
    {
        // Super Admin gets all permissions
      var superAdminPermissions = GetSuperAdminPermissions();
EnsureRole(sql, SystemRoleConstants.SuperAdminId, SystemRoleConstants.SuperAdminCode, "Super Administrator", superAdminPermissions);
     
   // Admin gets all permissions
        var adminPermissions = GetAdminPermissions();
        EnsureRole(sql, SystemRoleConstants.AdminId, SystemRoleConstants.AdminCode, "Administrator", adminPermissions);
        
      // Operator gets limited permissions
   var operatorPermissions = GetOperatorPermissions();
   EnsureRole(sql, SystemRoleConstants.OperatorId, SystemRoleConstants.OperatorCode, "Operator", operatorPermissions);
    }

    private static List<string> GetSuperAdminPermissions()
    {
// SuperAdmin has all permissions
return new List<string>
        {
  Functions.App_Add,
 Functions.App_Edit,
     Functions.App_Delete,
    Functions.App_Auth,

  Functions.Config_Add,
 Functions.Config_Edit,
       Functions.Config_Delete,
 Functions.Config_Publish,
 Functions.Config_Offline,

 Functions.Node_Add,
  Functions.Node_Delete,

          Functions.Client_Disconnect,

        Functions.User_Add,
    Functions.User_Edit,
  Functions.User_Delete,

  Functions.Role_Add,
 Functions.Role_Edit,
            Functions.Role_Delete
     };
    }

    private static List<string> GetAdminPermissions()
    {
        // Admin has all permissions same as SuperAdmin
        return GetSuperAdminPermissions();
    }

    private static List<string> GetOperatorPermissions()
    {
        // Operator has limited permissions:
        // - App: Add, Edit
        // - Config: Add, Edit, Delete, Publish, Offline
   return new List<string>
        {
     Functions.App_Add,
   Functions.App_Edit,

   Functions.Config_Add,
Functions.Config_Edit,
   Functions.Config_Delete,
      Functions.Config_Publish,
     Functions.Config_Offline
 };
    }

    private static void EnsureRole(IFreeSql sql, string id, string code, string name, List<string> functions)
    {
  var role = sql.Select<Role>().Where(x => x.Id == id).First();
    var functionsJson = JsonSerializer.Serialize(functions);

    if (role == null)
     {
      sql.Insert(new Role
  {
         Id = id,
         Code = code,
Name = name,
 Description = name,
       IsSystem = true,
 FunctionsJson = functionsJson,
    CreateTime = DateTime.Now
       }).ExecuteAffrows();
   }
    else
     {
     role.Code = code;
      role.Name = name;
        role.Description = name;
  role.IsSystem = true;
  role.FunctionsJson = functionsJson;
      role.UpdateTime = DateTime.Now;
    sql.Update<Role>().SetSource(role).ExecuteAffrows();
        }
    }
}
