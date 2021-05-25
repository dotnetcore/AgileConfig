using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.IService
{
    public class Functions
    {
        public const string User_Add = "User_Add";
        public const string User_Edit = "User_Edit";
        public const string User_Delete = "User_Delete";

        public const string Team_Add = "Team_Add";
        public const string Team_Edit = "Team_Edit";
        public const string Team_Delete = "Team_Delete";

        public const string App_Add = "App_Add";
        public const string App_Edit = "App_Edit";
        public const string App_Delete = "App_Delete";

        public const string Config_Add = "Config_Add";
        public const string Config_Edit = "Config_Edit";
        public const string Config_Delete = "Config_Delete";

        public const string Config_Publish = "Config_Publish";
        public const string Config_Offline = "Config_Offline";

    }

    public interface IPremissionService
    {
        List<string> GetRoleFunctions(Role role);
        List<string> GetRolesFunctions(List<Role> roles);
    }

}
