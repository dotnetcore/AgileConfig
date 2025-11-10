using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.IService;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync();
    Task<Role> GetAsync(string id);
    Task<Role> CreateAsync(Role role, IEnumerable<string> functions);
    Task<bool> UpdateAsync(Role role, IEnumerable<string> functions);
    Task<bool> DeleteAsync(string id);
}