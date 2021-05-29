using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System.Collections.Generic;

namespace AgileConfig.Server.Service
{
    public class PremissionService : IPremissionService
    {
        private static Dictionary<Role, List<string>> _roleFunctions = new Dictionary<Role, List<string>>() {
            {
                Role.SuperAdmin, new List<string>{
               
                }
            },
            {
               Role.Admin, new List<string>{

                }
            },
            {
                Role.NormalUser, new List<string>{
                 
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
