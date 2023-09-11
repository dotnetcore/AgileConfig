using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public class Functions
    {
        public const string App_Add = "APP_ADD";
        public const string App_Edit = "APP_EDIT";
        public const string App_Delete = "APP_DELETE";
        public const string App_Auth = "APP_AUTH";

        public const string Config_Add = "CONFIG_ADD";
        public const string Config_Edit = "CONFIG_EDIT";
        public const string Config_Delete = "CONFIG_DELETE";

        public const string Config_Publish = "CONFIG_PUBLISH";
        public const string Config_Offline = "CONFIG_OFFLINE";

        public const string Node_Add = "NODE_ADD";
        public const string Node_Delete = "NODE_DELETE";

        public const string Client_Disconnect = "CLIENT_DISCONNECT";

        public const string User_Add = "USER_ADD";
        public const string User_Edit = "USER_EDIT";
        public const string User_Delete = "USER_DELETE";
    }

    public interface IPremissionService
    {
         string EditConfigPermissionKey { get; }

         string PublishConfigPermissionKey { get; }

        Task<List<string>> GetUserPermission(string userId);
    }

}
