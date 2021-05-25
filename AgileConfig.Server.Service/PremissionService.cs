using AgileConfig.Server.Data.Entity;
using System.Collections.Generic;

namespace AgileConfig.Server.Service
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

    public class PremissionService
    {
        private static Dictionary<Role, List<string>> _roleFunctions = new Dictionary<Role, List<string>>() {
            {
                Role.SystemAdmin, new List<string>{
                    Functions.App_Add,
                    Functions.App_Delete,
                    Functions.App_Edit,
                    Functions.Team_Add,
                    Functions.Team_Delete,
                    Functions.Team_Edit,
                    Functions.User_Add,
                    Functions.User_Edit,
                    Functions.User_Delete,
                    Functions.Config_Add,
                    Functions.Config_Delete,
                    Functions.Config_Edit,
                    Functions.Config_Offline,
                    Functions.Config_Publish
                }
            },
            {
                Role.AppAdmin, new List<string>{
                    Functions.App_Edit,
                    Functions.Config_Add,
                    Functions.Config_Delete,
                    Functions.Config_Edit,
                    Functions.Config_Offline,
                    Functions.Config_Publish
                }
            },
            {
                Role.Editor, new List<string>{
                    Functions.Config_Add,
                    Functions.Config_Delete,
                    Functions.Config_Edit,
                }
            },
            {
                Role.Publisher, new List<string>{
                    Functions.Config_Offline,
                    Functions.Config_Publish
                }
            }
        };
        public List<string> GetRoleFunctions(Role role)
        {
            if (_roleFunctions.ContainsKey(role))
            {
                return _roleFunctions[role];
            }

            return new List<string>();
        }


        public List<string> GetRolesFunctions(List<Role> roles)
        {
            var functions = new List<string>();

            roles.ForEach(r=> {
                GetRoleFunctions(r).ForEach(f=> {
                    if (!functions.Contains(f))
                    {
                        functions.Add(f);
                    }
                });

            });

            return functions;
        }
    }
}
