using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service;

public class PermissionService : IPermissionService
{
    private readonly IAppRepository _appRepository;
    private readonly IFunctionRepository _functionRepository;
    private readonly IRoleDefinitionRepository _roleDefinitionRepository;
    private readonly IRoleFunctionRepository _roleFunctionRepository;
    private readonly IUserAppAuthRepository _userAppAuthRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public PermissionService(
        IUserRoleRepository userRoleRepository,
        IRoleDefinitionRepository roleDefinitionRepository,
        IRoleFunctionRepository roleFunctionRepository,
        IFunctionRepository functionRepository,
        IUserAppAuthRepository userAppAuthRepository,
        IAppRepository appRepository)
    {
        _userRoleRepository = userRoleRepository;
        _roleDefinitionRepository = roleDefinitionRepository;
        _roleFunctionRepository = roleFunctionRepository;
        _functionRepository = functionRepository;
        _userAppAuthRepository = userAppAuthRepository;
        _appRepository = appRepository;
    }


    /// <summary>
    ///     Retrieve the permission codes for a user based on roles.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting permissions.</param>
    /// <returns>List of permission codes granted to the user.</returns>
    public async Task<List<string>> GetUserPermission(string userId)
    {
        var userRoles = await _userRoleRepository.QueryAsync(x => x.UserId == userId);
        var roleIds = userRoles.Select(x => x.RoleId).Distinct().ToList();
        if (!roleIds.Any()) return new List<string>();

        // Get role-function mappings for all user roles
        var roleFunctions = await _roleFunctionRepository.QueryAsync(x => roleIds.Contains(x.RoleId));
        var functionIds = roleFunctions.Select(rf => rf.FunctionId).Distinct().ToList();

// Get function entities and return their codes
        var functions = await _functionRepository.QueryAsync(f => functionIds.Contains(f.Id));
        var functionCodes = functions.Select(f => f.Code).Distinct().ToList();

        return functionCodes;
    }

    /// <summary>
    ///     Retrieve the categories of permissions granted to a user.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting categories.</param>
    /// <returns>List of distinct categories granted to the user.</returns>
    public async Task<List<string>> GetUserCategories(string userId)
    {
        var userRoles = await _userRoleRepository.QueryAsync(x => x.UserId == userId);
        var roleIds = userRoles.Select(x => x.RoleId).Distinct().ToList();
        if (!roleIds.Any()) return new List<string>();

        // Get role-function mappings for all user roles
        var roleFunctions = await _roleFunctionRepository.QueryAsync(x => roleIds.Contains(x.RoleId));
        var functionIds = roleFunctions.Select(rf => rf.FunctionId).Distinct().ToList();

        // Get function entities and return their categories
        var functions = await _functionRepository.QueryAsync(f => functionIds.Contains(f.Id));
        var categories = functions
            .Where(f => !string.IsNullOrWhiteSpace(f.Category))
            .Select(f => f.Category)
            .Distinct()
            .ToList();

        return categories;
    }

    public string EditConfigPermissionKey => "EDIT_CONFIG";

    public string PublishConfigPermissionKey => "PUBLISH_CONFIG";

    /// <summary>
    ///     Retrieve applications where the user has been explicitly authorized.
    /// </summary>
    /// <param name="userId">Identifier of the user whose application authorizations are requested.</param>
    /// <param name="authPermissionKey">Permission key used to filter authorized applications.</param>
    /// <returns>List of applications the user can access for the specified permission.</returns>
    private async Task<List<App>> GetUserAuthApp(string userId, string authPermissionKey)
    {
        var apps = new List<App>();
        var userAuths =
            await _userAppAuthRepository.QueryAsync(x => x.UserId == userId && x.Permission == authPermissionKey);
        foreach (var appAuth in userAuths)
        {
            var app = await _appRepository.GetAsync(appAuth.AppId);
            if (app != null) apps.Add(app);
        }

        return apps;
    }

    /// <summary>
    ///     Retrieve applications managed by the user.
    /// </summary>
    /// <param name="userId">Identifier of the user who administers the applications.</param>
    /// <returns>List of applications where the user is the administrator.</returns>
    private async Task<List<App>> GetUserAdminApps(string userId)
    {
        return await _appRepository.QueryAsync(x => x.AppAdmin == userId);
    }
}