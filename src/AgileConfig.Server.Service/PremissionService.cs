using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AgileConfig.Server.Service
{
    public class PermissionService : IPremissionService
    {
        private FreeSqlContext _dbContext;
        public PermissionService(FreeSqlContext freeSql)
        {
            _dbContext = freeSql;
        }

        private static readonly List<string> Template_SuperAdminPermissions = new List<string>
        {
            "GLOBAL_" + Functions.App_Add,
            "GLOBAL_" + Functions.App_Delete,
            "GLOBAL_" + Functions.App_Edit,
            "GLOBAL_" + Functions.App_Auth,

            "GLOBAL_" + Functions.Config_Add,
            "GLOBAL_" + Functions.Config_Delete,
            "GLOBAL_" + Functions.Config_Edit,
            "GLOBAL_" + Functions.Config_Offline,
            "GLOBAL_" + Functions.Config_Publish,

            "GLOBAL_" + Functions.Node_Add,
            "GLOBAL_" + Functions.Node_Delete,

            "GLOBAL_" + Functions.Client_Disconnect,

            "GLOBAL_" + Functions.User_Add,
            "GLOBAL_" + Functions.User_Edit,
            "GLOBAL_" + Functions.User_Delete,
        };

        private static readonly List<string> Template_NormalAdminPermissions = new List<string>
        {
            "GLOBAL_" + Functions.App_Add,
            "GLOBAL_" + Functions.Node_Add,
            "GLOBAL_" + Functions.Node_Delete,
            "GLOBAL_" + Functions.Client_Disconnect,

            "GLOBAL_" + Functions.User_Add,
            "GLOBAL_" + Functions.User_Edit,
            "GLOBAL_" + Functions.User_Delete,

            "APP_{0}_" + Functions.App_Delete,
            "APP_{0}_" + Functions.App_Edit,
            "APP_{0}_" + Functions.App_Auth,

            "APP_{0}_" + Functions.Config_Add,
            "APP_{0}_" + Functions.Config_Delete,
            "APP_{0}_" + Functions.Config_Edit,
            "APP_{0}_" + Functions.Config_Offline,
            "APP_{0}_" + Functions.Config_Publish,
        };

        private static readonly List<string> Template_NormalUserPermissions_Edit = new List<string>
        {
            "APP_{0}_" + Functions.Config_Add,
            "APP_{0}_" + Functions.Config_Delete,
            "APP_{0}_" + Functions.Config_Edit,
        };

        private static readonly List<string> Template_NormalUserPermissions_Publish = new List<string>
        {
            "APP_{0}_" + Functions.Config_Offline,
            "APP_{0}_" + Functions.Config_Publish
        };

        private async Task<List<string>> GetAdminUserFunctions(string userId)
        {
            var userFunctions = new List<string>();
            //获取当前用户为管理员的app
            var adminApps = await GetUserAdminApps(userId);
            Template_NormalAdminPermissions.Where(x => x.StartsWith("GLOBAL_")).ToList().ForEach(
                 key => {
                     userFunctions.Add(key);
                 }
                );
            Template_NormalUserPermissions_Edit.Where(x => x.StartsWith("GLOBAL_")).ToList().ForEach(
                 key => {
                     userFunctions.Add(key);
                 }
                );
            Template_NormalUserPermissions_Publish.Where(x => x.StartsWith("GLOBAL_")).ToList().ForEach(
                 key => {
                     userFunctions.Add(key);
                 }
                );
            foreach (var app in adminApps)
            {
                foreach (var temp in Template_NormalAdminPermissions)
                {
                    if (temp.StartsWith("APP_{0}_"))
                    {
                        userFunctions.Add(string.Format(temp, app.Id));
                    }
                }
            }
            //EditConfigPermissionKey
            var editPermissionApps = await GetUserAuthApp(userId, EditConfigPermissionKey);
            foreach (var app in editPermissionApps)
            {
                foreach (var temp in Template_NormalUserPermissions_Edit)
                {
                    if (temp.StartsWith("APP_{0}_"))
                    {
                        userFunctions.Add(string.Format(temp, app.Id));
                    }
                }
            }
            //PublishConfigPermissionKey
            var publishPermissionApps = await GetUserAuthApp(userId, PublishConfigPermissionKey);
            foreach (var app in publishPermissionApps)
            {
                foreach (var temp in Template_NormalUserPermissions_Publish)
                {
                    if (temp.StartsWith("APP_{0}_"))
                    {
                        userFunctions.Add(string.Format(temp, app.Id));
                    }
                }
            }

            return userFunctions;
        }

        private async Task<List<string>> GetNormalUserFunctions(string userId)
        {
            var userFunctions = new List<string>();
            //EditConfigPermissionKey
            var editPermissionApps = await GetUserAuthApp(userId, EditConfigPermissionKey);
            foreach (var app in editPermissionApps)
            {
                foreach (var temp in Template_NormalUserPermissions_Edit)
                {
                    if (temp.StartsWith("GLOBAL_"))
                    {
                        userFunctions.Add(temp);
                    }
                    if (temp.StartsWith("APP_{0}_"))
                    {
                        userFunctions.Add(string.Format(temp, app.Id));
                    }
                }
            }
            //PublishConfigPermissionKey
            var publishPermissionApps = await GetUserAuthApp(userId, PublishConfigPermissionKey);
            foreach (var app in publishPermissionApps)
            {
                foreach (var temp in Template_NormalUserPermissions_Publish)
                {
                    if (temp.StartsWith("GLOBAL_"))
                    {
                        userFunctions.Add(temp);
                    }
                    if (temp.StartsWith("APP_{0}_"))
                    {
                        userFunctions.Add(string.Format(temp, app.Id));
                    }
                }
            }

            return userFunctions;
        }

        /// <summary>
        /// 获取角色权限的模板
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<List<string>> GetUserPermission(string userId)
        {
            var userRoles = await _dbContext.UserRoles.Where(x => x.UserId == userId).ToListAsync();
            if (userRoles.Any(x=>x.Role == Role.SuperAdmin))
            {
                return Template_SuperAdminPermissions;
            }

            var userFunctions = new List<string>();
            //计算普通管理员的权限
            if (userRoles.Any(x=>x.Role == Role.Admin))
            {
                userFunctions.AddRange(await GetAdminUserFunctions(userId));
            }
            //计算普通用户的权限
            if (userRoles.Any(x => x.Role == Role.NormalUser))
            {
                userFunctions.AddRange(await GetNormalUserFunctions(userId));
            }

            return userFunctions.Distinct().ToList();
        }

        /// <summary>
        /// 获取被授权给用户的app
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<List<App>> GetUserAuthApp(string userId, string authPermissionKey)
        {
            var apps = new List<App>();
            var userAuths = await _dbContext.UserAppAuths.Where(x => x.UserId == userId && x.Permission == authPermissionKey).ToListAsync();
            foreach (var appAuth in userAuths)
            {
                var app = await _dbContext.Apps.Where(x=>x.Id == appAuth.AppId).FirstAsync();
                if (app!= null)
                {
                    apps.Add(app);
                }
            }

            return apps;
        }

        /// <summary>
        /// 获取是管理员的app
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<List<App>> GetUserAdminApps(string userId)
        {
            return await _dbContext.Apps.Where(x => x.AppAdmin == userId).ToListAsync();
        }

        public string EditConfigPermissionKey => "EDIT_CONFIG";

        public string PublishConfigPermissionKey => "PUBLISH_CONFIG";
    }
}
