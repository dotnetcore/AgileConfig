using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class RoleService : IRoleService
    {
        private readonly IRoleDefinitionRepository _roleDefinitionRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public RoleService(IRoleDefinitionRepository roleDefinitionRepository, IUserRoleRepository userRoleRepository)
        {
            _roleDefinitionRepository = roleDefinitionRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<RoleDefinition> CreateAsync(RoleDefinition role, IEnumerable<string> functions)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (string.IsNullOrWhiteSpace(role.Id))
            {
                role.Id = Guid.NewGuid().ToString("N");
            }

            if (await ExistsWithSameCode(role))
            {
                throw new InvalidOperationException($"Role code '{role.Code}' already exists.");
            }

            role.CreateTime = DateTime.Now;
            role.FunctionsJson = SerializeFunctions(functions);

            await _roleDefinitionRepository.InsertAsync(role);
            return role;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var role = await _roleDefinitionRepository.GetAsync(id);
            if (role == null)
            {
                return false;
            }

            if (role.IsSystem)
            {
                throw new InvalidOperationException("System roles cannot be deleted.");
            }

            var userRoles = await _userRoleRepository.QueryAsync(x => x.RoleId == id);
            if (userRoles.Any())
            {
                await _userRoleRepository.DeleteAsync(userRoles);
            }

            await _roleDefinitionRepository.DeleteAsync(role);
            return true;
        }

        public async Task<List<RoleDefinition>> GetAllAsync()
        {
            return await _roleDefinitionRepository.AllAsync();
        }

        public Task<RoleDefinition> GetAsync(string id)
        {
            return _roleDefinitionRepository.GetAsync(id);
        }

        public async Task<RoleDefinition> GetByCodeAsync(string code)
        {
            var roles = await _roleDefinitionRepository.QueryAsync(x => x.Code == code);
            return roles.FirstOrDefault();
        }

        public async Task<bool> UpdateAsync(RoleDefinition role, IEnumerable<string> functions)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var dbRole = await _roleDefinitionRepository.GetAsync(role.Id);
            if (dbRole == null)
            {
                return false;
            }

            if (dbRole.IsSystem && !string.Equals(dbRole.Code, role.Code, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("System role code cannot be changed.");
            }

            if (!string.Equals(dbRole.Code, role.Code, StringComparison.OrdinalIgnoreCase) && await ExistsWithSameCode(role))
            {
                throw new InvalidOperationException($"Role code '{role.Code}' already exists.");
            }

            dbRole.Code = role.Code;
            dbRole.Name = role.Name;
            dbRole.Description = role.Description;
            dbRole.IsSystem = role.IsSystem;
            dbRole.FunctionsJson = SerializeFunctions(functions);
            dbRole.UpdateTime = DateTime.Now;

            await _roleDefinitionRepository.UpdateAsync(dbRole);
            return true;
        }

        private async Task<bool> ExistsWithSameCode(RoleDefinition role)
        {
            if (string.IsNullOrWhiteSpace(role.Code))
            {
                return false;
            }

            var sameCodeRoles = await _roleDefinitionRepository.QueryAsync(x => x.Code == role.Code);
            return sameCodeRoles.Any(x => !string.Equals(x.Id, role.Id, StringComparison.OrdinalIgnoreCase));
        }

        private static string SerializeFunctions(IEnumerable<string> functions)
        {
            var normalized = functions?.Where(f => !string.IsNullOrWhiteSpace(f)).Select(f => f.Trim()).Distinct().ToList()
                            ?? new List<string>();

            return JsonSerializer.Serialize(normalized);
        }
    }
}
