using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Application.Roles;

public sealed class RoleManagementService : IRoleManagementService
{
    private readonly IRoleService _roleService;

    public RoleManagementService(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IReadOnlyList<RoleDetails>> GetAllAsync()
    {
        var roles = (await _roleService.GetAllAsync())
            .Where(x => x.Id != SystemRoleConstants.SuperAdminId)
            .OrderByDescending(x => x.IsSystem)
            .ThenBy(x => x.Name)
            .ToList();
        var details = new List<RoleDetails>(roles.Count);
        foreach (var role in roles)
            details.Add(new RoleDetails(role, await _roleService.GetFunctionsAsync(role.Id)));

        return details;
    }

    public IReadOnlyList<string> GetSupportedPermissions()
    {
        return Functions.GetAllPermissions();
    }

    public async Task<ApplicationResult<Role>> CreateAsync(CreateRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!string.IsNullOrWhiteSpace(command.Id) && await _roleService.GetAsync(command.Id) != null)
            return ApplicationResult<Role>.Failure(ApplicationError.Conflict);

        var role = new Role
        {
            Id = command.Id,
            Name = command.Name,
            Description = command.Description ?? string.Empty,
            IsSystem = false
        };
        var created = await _roleService.CreateAsync(role, NormalizeFunctions(command.Functions));
        return ApplicationResult<Role>.Success(created);
    }

    public async Task<ApplicationResult<Role>> UpdateAsync(UpdateRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var existing = await _roleService.GetAsync(command.Id);
        if (existing == null) return ApplicationResult<Role>.Failure(ApplicationError.NotFound);
        if (existing.Id == SystemRoleConstants.SuperAdminId)
            return ApplicationResult<Role>.Failure(ApplicationError.Forbidden);

        var role = new Role
        {
            Id = existing.Id,
            Name = command.Name,
            Description = command.Description ?? string.Empty,
            IsSystem = existing.IsSystem
        };
        var succeeded = await _roleService.UpdateAsync(role, NormalizeFunctions(command.Functions));
        return succeeded
            ? ApplicationResult<Role>.Success(role)
            : ApplicationResult<Role>.Failure(ApplicationError.OperationFailed);
    }

    public async Task<ApplicationResult> DeleteAsync(string id)
    {
        var existing = await _roleService.GetAsync(id);
        if (existing == null) return ApplicationResult.Failure(ApplicationError.NotFound);
        if (existing.IsSystem) return ApplicationResult.Failure(ApplicationError.Forbidden);

        return await _roleService.DeleteAsync(id)
            ? ApplicationResult.Success()
            : ApplicationResult.Failure(ApplicationError.OperationFailed);
    }

    private static IReadOnlyList<string> NormalizeFunctions(IReadOnlyList<string> functions)
    {
        return functions?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList() ?? [];
    }
}
