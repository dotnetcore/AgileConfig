using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service;

public class RoleService : IRoleService
{
    private readonly IRoleDefinitionRepository _roleDefinitionRepository;
    private readonly IRoleFunctionRepository _roleFunctionRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public RoleService(
        IRoleDefinitionRepository roleDefinitionRepository,
        IUserRoleRepository userRoleRepository,
        IRoleFunctionRepository roleFunctionRepository)
    {
        _roleDefinitionRepository = roleDefinitionRepository;
        _userRoleRepository = userRoleRepository;
        _roleFunctionRepository = roleFunctionRepository;
    }

    public async Task<Role> CreateAsync(Role role, IEnumerable<string> functions)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        if (string.IsNullOrWhiteSpace(role.Id)) role.Id = Guid.NewGuid().ToString("N");

        role.CreateTime = DateTime.Now;

        await _roleDefinitionRepository.InsertAsync(role);

        // Save role-function relationships
        if (functions != null && functions.Any())
        {
            var roleFunctions = functions
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => new RoleFunction
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RoleId = role.Id,
                    FunctionId = f,
                    CreateTime = DateTime.Now
                })
                .ToList();

            if (roleFunctions.Any()) await _roleFunctionRepository.InsertAsync(roleFunctions);
        }

        return role;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var role = await _roleDefinitionRepository.GetAsync(id);
        if (role == null) return false;

        if (role.IsSystem) throw new InvalidOperationException("System roles cannot be deleted.");

        // Delete user-role relationships
        var userRoles = await _userRoleRepository.QueryAsync(x => x.RoleId == id);
        if (userRoles.Any()) await _userRoleRepository.DeleteAsync(userRoles);

        // Delete role-function relationships
        var roleFunctions = await _roleFunctionRepository.QueryAsync(x => x.RoleId == id);
        if (roleFunctions.Any()) await _roleFunctionRepository.DeleteAsync(roleFunctions);

        await _roleDefinitionRepository.DeleteAsync(role);
        return true;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _roleDefinitionRepository.AllAsync();
    }

    public Task<Role> GetAsync(string id)
    {
        return _roleDefinitionRepository.GetAsync(id);
    }

    public async Task<bool> UpdateAsync(Role role, IEnumerable<string> functions)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        var dbRole = await _roleDefinitionRepository.GetAsync(role.Id);
        if (dbRole == null) return false;

        dbRole.Name = role.Name;
        dbRole.Description = role.Description;
        dbRole.IsSystem = role.IsSystem;
        dbRole.UpdateTime = DateTime.Now;

        await _roleDefinitionRepository.UpdateAsync(dbRole);

        // Update role-function relationships
        var existingRoleFunctions = await _roleFunctionRepository.QueryAsync(x => x.RoleId == role.Id);
        if (existingRoleFunctions.Any()) await _roleFunctionRepository.DeleteAsync(existingRoleFunctions);

        if (functions != null && functions.Any())
        {
            var roleFunctions = functions
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => new RoleFunction
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RoleId = role.Id,
                    FunctionId = f,
                    CreateTime = DateTime.Now
                })
                .ToList();

            if (roleFunctions.Any()) await _roleFunctionRepository.InsertAsync(roleFunctions);
        }

        return true;
    }
}