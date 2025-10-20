using AgileConfig.Server.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IRoleService
    {
        Task<List<RoleDefinition>> GetAllAsync();
        Task<RoleDefinition> GetAsync(string id);
        Task<RoleDefinition> GetByCodeAsync(string code);
        Task<RoleDefinition> CreateAsync(RoleDefinition role, IEnumerable<string> functions);
        Task<bool> UpdateAsync(RoleDefinition role, IEnumerable<string> functions);
        Task<bool> DeleteAsync(string id);
    }
}
