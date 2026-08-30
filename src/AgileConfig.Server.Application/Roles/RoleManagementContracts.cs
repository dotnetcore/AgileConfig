using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Application.Roles;

public sealed record CreateRoleCommand(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string> Functions);

public sealed record UpdateRoleCommand(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string> Functions);

public sealed record RoleDetails(Role Role, IReadOnlyList<string> Functions);

public interface IRoleManagementService
{
    Task<IReadOnlyList<RoleDetails>> GetAllAsync();

    IReadOnlyList<string> GetSupportedPermissions();

    Task<ApplicationResult<Role>> CreateAsync(CreateRoleCommand command);

    Task<ApplicationResult<Role>> UpdateAsync(UpdateRoleCommand command);

    Task<ApplicationResult> DeleteAsync(string id);
}
