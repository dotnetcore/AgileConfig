using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Application.Users;

public sealed class UserManagementService : IUserManagementService
{
    private const string DefaultPassword = "123456";
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITinyEventBus _eventBus;
    private readonly TimeProvider _timeProvider;
    private readonly IUserService _userService;

    public UserManagementService(
        IUserService userService,
        ITinyEventBus eventBus,
        ICurrentUserAccessor currentUserAccessor,
        TimeProvider timeProvider)
    {
        _userService = userService;
        _eventBus = eventBus;
        _currentUserAccessor = currentUserAccessor;
        _timeProvider = timeProvider;
    }

    public async Task<UserSearchResult> SearchAsync(SearchUsersQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Current <= 0 || query.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(query));

        var users = (await _userService.GetAll())
            .Where(IsVisibleUser)
            .Where(x => string.IsNullOrEmpty(query.UserName) ||
                        x.UserName != null && x.UserName.Contains(query.UserName))
            .Where(x => string.IsNullOrEmpty(query.Team) ||
                        x.Team != null && x.Team.Contains(query.Team))
            .OrderByDescending(x => x.CreateTime)
            .ToList();

        var page = users
            .Skip((query.Current - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();
        var details = new List<UserDetails>(page.Count);
        foreach (var user in page)
        {
            var roles = await _userService.GetUserRolesAsync(user.Id);
            details.Add(new UserDetails(
                user,
                roles.Select(x => x.Id).ToList(),
                roles.Select(x => x.Name).ToList()));
        }

        return new UserSearchResult(query.Current, query.PageSize, users.Count, details);
    }

    public async Task<ApplicationResult<User>> CreateAsync(CreateUserCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existingUsers = await _userService.GetUsersByNameAsync(command.UserName);
        if (existingUsers.Any(x => x.Status == UserStatus.Normal))
            return ApplicationResult<User>.Failure(ApplicationError.Conflict);

        var salt = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Salt = salt,
            Password = Encrypt.Md5(command.Password + salt),
            Status = UserStatus.Normal,
            Team = command.Team,
            CreateTime = GetLocalNow(),
            UserName = command.UserName
        };

        var userCreated = await _userService.AddAsync(user);
        var rolesUpdated = await _userService.UpdateUserRolesAsync(user.Id, NormalizeRoleIds(command.RoleIds));
        if (userCreated) _eventBus.Fire(new AddUserSuccessful(user, _currentUserAccessor.UserName));

        return userCreated && rolesUpdated
            ? ApplicationResult<User>.Success(user)
            : ApplicationResult<User>.Failure(ApplicationError.OperationFailed, user);
    }

    public async Task<ApplicationResult<User>> UpdateAsync(UpdateUserCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var user = await _userService.GetUserAsync(command.Id);
        if (user == null) return ApplicationResult<User>.Failure(ApplicationError.NotFound);

        user.Team = command.Team;
        user.UpdateTime = GetLocalNow();

        var userUpdated = await _userService.UpdateAsync(user);
        var rolesUpdated = await _userService.UpdateUserRolesAsync(user.Id, NormalizeRoleIds(command.RoleIds));
        if (userUpdated) _eventBus.Fire(new EditUserSuccessful(user, _currentUserAccessor.UserName));

        return userUpdated && rolesUpdated
            ? ApplicationResult<User>.Success(user)
            : ApplicationResult<User>.Failure(ApplicationError.OperationFailed, user);
    }

    public async Task<ApplicationResult<User>> ResetPasswordAsync(string userId)
    {
        var user = await _userService.GetUserAsync(userId);
        if (user == null) return ApplicationResult<User>.Failure(ApplicationError.NotFound);

        user.Password = Encrypt.Md5(DefaultPassword + user.Salt);
        var succeeded = await _userService.UpdateAsync(user);
        if (!succeeded) return ApplicationResult<User>.Failure(ApplicationError.OperationFailed, user);

        _eventBus.Fire(new ResetUserPasswordSuccessful(_currentUserAccessor.UserName, user.UserName));
        return ApplicationResult<User>.Success(user);
    }

    public async Task<ApplicationResult<User>> DeleteAsync(string userId)
    {
        var user = await _userService.GetUserAsync(userId);
        if (user == null) return ApplicationResult<User>.Failure(ApplicationError.NotFound);

        user.Status = UserStatus.Deleted;
        var succeeded = await _userService.UpdateAsync(user);
        if (!succeeded) return ApplicationResult<User>.Failure(ApplicationError.OperationFailed, user);

        _eventBus.Fire(new DeleteUserSuccessful(user, _currentUserAccessor.UserName));
        return ApplicationResult<User>.Success(user);
    }

    public async Task<IReadOnlyList<User>> GetAdministratorsAsync()
    {
        return (await _userService.GetUsersByRoleAsync(SystemRoleConstants.AdminId))
            .Where(x => x.Status == UserStatus.Normal)
            .OrderBy(x => x.Team)
            .ThenBy(x => x.UserName)
            .ToList();
    }

    public async Task<IReadOnlyList<User>> GetAllActiveAsync()
    {
        return (await _userService.GetAll())
            .Where(IsVisibleUser)
            .OrderBy(x => x.Team)
            .ThenBy(x => x.UserName)
            .ToList();
    }

    private DateTime GetLocalNow()
    {
        return _timeProvider.GetLocalNow().DateTime;
    }

    private static bool IsVisibleUser(User user)
    {
        return user.Status == UserStatus.Normal && user.Id != SystemSettings.SuperAdminId;
    }

    private static List<string> NormalizeRoleIds(IReadOnlyList<string> roleIds)
    {
        var normalized = roleIds?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList() ?? [];
        if (normalized.Count == 0) normalized.Add(SystemRoleConstants.OperatorId);
        return normalized;
    }
}
