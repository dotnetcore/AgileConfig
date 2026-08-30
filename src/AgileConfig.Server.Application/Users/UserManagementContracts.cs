using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Application.Users;

public sealed record SearchUsersQuery(
    string UserName,
    string Team,
    int Current,
    int PageSize);

public sealed record CreateUserCommand(
    string UserName,
    string Password,
    string Team,
    IReadOnlyList<string> RoleIds);

public sealed record UpdateUserCommand(
    string Id,
    string Team,
    IReadOnlyList<string> RoleIds);

public sealed record UserDetails(
    User User,
    IReadOnlyList<string> RoleIds,
    IReadOnlyList<string> RoleNames);

public sealed record UserSearchResult(
    int Current,
    int PageSize,
    int Total,
    IReadOnlyList<UserDetails> Users);

public interface IUserManagementService
{
    Task<UserSearchResult> SearchAsync(SearchUsersQuery query);

    Task<ApplicationResult<User>> CreateAsync(CreateUserCommand command);

    Task<ApplicationResult<User>> UpdateAsync(UpdateUserCommand command);

    Task<ApplicationResult<User>> ResetPasswordAsync(string userId);

    Task<ApplicationResult<User>> DeleteAsync(string userId);

    Task<IReadOnlyList<User>> GetAdministratorsAsync();

    Task<IReadOnlyList<User>> GetAllActiveAsync();
}
