using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
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
        EnsureRole(sql, SystemRoleConstants.SuperAdminId, SystemRoleConstants.SuperAdminCode, "Super Administrator");
        EnsureRole(sql, SystemRoleConstants.AdminId, SystemRoleConstants.AdminCode, "Administrator");
        EnsureRole(sql, SystemRoleConstants.OperatorId, SystemRoleConstants.OperatorCode, "Operator");
    }

    private static void EnsureRole(IFreeSql sql, string id, string code, string name)
    {
        var role = sql.Select<RoleDefinition>().Where(x => x.Id == id).First();
        if (role == null)
        {
            sql.Insert(new RoleDefinition
            {
                Id = id,
                Code = code,
                Name = name,
                Description = name,
                IsSystem = true,
                FunctionsJson = JsonSerializer.Serialize(new List<string>()),
                CreateTime = DateTime.Now
            }).ExecuteAffrows();
        }
        else
        {
            role.Code = code;
            role.Name = name;
            role.Description = name;
            role.IsSystem = true;
            sql.Update<RoleDefinition>().SetSource(role).ExecuteAffrows();
        }
    }
}