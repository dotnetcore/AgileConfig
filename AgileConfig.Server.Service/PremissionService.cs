using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System.Collections.Generic;

namespace AgileConfig.Server.Service
{
    public class PremissionService : IPremissionService
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
            var functions = new List<string>();

            if (_roleFunctions.ContainsKey(role))
            {
                _roleFunctions[role].ForEach(x => functions.Add(x));
            }

            return functions;
        }


        public List<string> GetRolesFunctions(List<Role> roles)
        {
            var functions = new List<string>();

            roles.ForEach(r =>
            {
                GetRoleFunctions(r).ForEach(f =>
                {
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
