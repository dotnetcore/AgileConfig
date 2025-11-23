using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service;

public class PermissionService : IPermissionService
{
    private readonly IFunctionRepository _functionRepository;
    private readonly IRoleFunctionRepository _roleFunctionRepository;
    private readonly IUserAppAuthRepository _userAppAuthRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public PermissionService(
        IUserRoleRepository userRoleRepository,
        IRoleFunctionRepository roleFunctionRepository,
        IFunctionRepository functionRepository,
        IUserAppAuthRepository userAppAuthRepository)
    {
        _userRoleRepository = userRoleRepository;
        _roleFunctionRepository = roleFunctionRepository;
        _functionRepository = functionRepository;
        _userAppAuthRepository = userAppAuthRepository;
    }


    /// <summary>
    ///     Retrieve the permission codes for a user based on roles.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting permissions.</param>
    /// <returns>List of permission codes granted to the user.</returns>
    public async Task<List<string>> GetUserPermission(string userId)
    {
        var functionCodes = new List<string>();

        var userRoles = await _userRoleRepository.QueryAsync(x => x.UserId == userId);
        var roleIds = userRoles.Select(x => x.RoleId).Distinct().ToList();
        if (roleIds.Any())
        {
            var roleFunctions = await _roleFunctionRepository.QueryAsync(x => roleIds.Contains(x.RoleId));
            var functionIds = roleFunctions.Select(rf => rf.FunctionId).Distinct().ToList();

            var functions = await _functionRepository.QueryAsync(f => functionIds.Contains(f.Id));
            functionCodes = functions.Select(f => f.Code).Distinct().ToList();
        }

        return functionCodes.Distinct().ToList();
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
}