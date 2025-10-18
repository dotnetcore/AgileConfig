﻿using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.Service
{
    public class PermissionService : IPermissionService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserAppAuthRepository _userAppAuthRepository;
        private readonly IAppRepository _appRepository;

        public PermissionService(
            IUserRoleRepository userRoleRepository,
            IUserAppAuthRepository userAppAuthRepository,
            IAppRepository appRepository)
        {
            _userRoleRepository = userRoleRepository;
            _userAppAuthRepository = userAppAuthRepository;
            _appRepository = appRepository;
        }

        private static readonly List<string> Template_SuperAdminPermissions =
        [
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
            "GLOBAL_" + Functions.User_Delete
        ];

        private static readonly List<string> Template_NormalAdminPermissions =
        [
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
            "APP_{0}_" + Functions.Config_Publish
        ];

        private static readonly List<string> Template_NormalUserPermissions_Edit =
        [
            "APP_{0}_" + Functions.Config_Add,
            "APP_{0}_" + Functions.Config_Delete,
            "APP_{0}_" + Functions.Config_Edit
        ];

        private static readonly List<string> Template_NormalUserPermissions_Publish =
        [
            "APP_{0}_" + Functions.Config_Offline,
            "APP_{0}_" + Functions.Config_Publish
        ];

        private async Task<List<string>> GetAdminUserFunctions(string userId)
        {
            var userFunctions = new List<string>();
            // Retrieve applications where the user is an administrator.
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
        /// Retrieve the permission template for a user based on roles.
        /// </summary>
        /// <param name="userId">Identifier of the user requesting permissions.</param>
        /// <returns>List of permission keys granted to the user.</returns>
        public async Task<List<string>> GetUserPermission(string userId)
        {
            var userRoles = await _userRoleRepository.QueryAsync(x => x.UserId == userId);
            if (userRoles.Any(x=>x.Role == Role.SuperAdmin))
            {
                return Template_SuperAdminPermissions;
            }

            var userFunctions = new List<string>();
            // Compute permissions for regular administrators.
            if (userRoles.Any(x=>x.Role == Role.Admin))
            {
                userFunctions.AddRange(await GetAdminUserFunctions(userId));
            }
            // Compute permissions for regular users.
            if (userRoles.Any(x => x.Role == Role.NormalUser))
            {
                userFunctions.AddRange(await GetNormalUserFunctions(userId));
            }

            return userFunctions.Distinct().ToList();
        }

        /// <summary>
        /// Retrieve applications where the user has been explicitly authorized.
        /// </summary>
        /// <param name="userId">Identifier of the user whose application authorizations are requested.</param>
        /// <param name="authPermissionKey">Permission key used to filter authorized applications.</param>
        /// <returns>List of applications the user can access for the specified permission.</returns>
        private async Task<List<App>> GetUserAuthApp(string userId, string authPermissionKey)
        {
            var apps = new List<App>();
            var userAuths =
                await _userAppAuthRepository.QueryAsync(x => x.UserId == userId && x.Permission == authPermissionKey);
            foreach (var appAuth in userAuths)
            {
                var app = await _appRepository.GetAsync(appAuth.AppId);
                if (app!= null)
                {
                    apps.Add(app);
                }
            }

            return apps;
        }

        /// <summary>
        /// Retrieve applications managed by the user.
        /// </summary>
        /// <param name="userId">Identifier of the user who administers the applications.</param>
        /// <returns>List of applications where the user is the administrator.</returns>
        private async Task<List<App>> GetUserAdminApps(string userId)
        {
            return await _appRepository.QueryAsync(x => x.AppAdmin == userId);
        }

        public string EditConfigPermissionKey => "EDIT_CONFIG";

        public string PublishConfigPermissionKey => "PUBLISH_CONFIG";
    }
}
