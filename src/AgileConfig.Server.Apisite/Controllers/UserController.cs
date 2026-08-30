using System;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Users;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UserController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.User_Read })]
    public async Task<IActionResult> Search(string userName, string team, int current = 1, int pageSize = 20)
    {
        if (current <= 0) throw new ArgumentException(Messages.CurrentCannotBeLessThanOneUser);
        if (pageSize <= 0) throw new ArgumentException(Messages.PageSizeCannotBeLessThanOneUser);

        var result = await _userManagementService.SearchAsync(
            new SearchUsersQuery(userName, team, current, pageSize));

        return Json(new
        {
            current = result.Current,
            pageSize = result.PageSize,
            success = true,
            total = result.Total,
            data = result.Users.Select(ToViewModel).ToList()
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.User_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] UserVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var result = await _userManagementService.CreateAsync(new CreateUserCommand(
            model.UserName,
            model.Password,
            model.Team,
            model.UserRoleIds));
        if (result.Error == ApplicationError.Conflict)
            return Json(new
            {
                success = false,
                message = Messages.UserAlreadyExists(model.UserName)
            });

        return Json(new
        {
            success = result.Succeeded,
            message = !result.Succeeded ? Messages.AddUserFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.User_Edit })]
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] UserVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var result = await _userManagementService.UpdateAsync(new UpdateUserCommand(
            model.Id,
            model.Team,
            model.UserRoleIds));
        if (result.Error == ApplicationError.NotFound)
            return Json(new
            {
                success = false,
                message = Messages.UserNotFoundForOperation
            });

        return Json(new
        {
            success = result.Succeeded,
            message = !result.Succeeded ? Messages.UpdateUserFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.User_Edit })]
    [HttpPost]
    public async Task<IActionResult> ResetPassword(string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException("userId");

        var result = await _userManagementService.ResetPasswordAsync(userId);
        if (result.Error == ApplicationError.NotFound)
            return Json(new
            {
                success = false,
                message = Messages.UserNotFoundForOperation
            });

        return Json(new
        {
            success = result.Succeeded,
            message = !result.Succeeded ? Messages.ResetUserPasswordFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.User_Delete })]
    [HttpPost]
    public async Task<IActionResult> Delete(string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

        var result = await _userManagementService.DeleteAsync(userId);
        if (result.Error == ApplicationError.NotFound)
            return Json(new
            {
                success = false,
                message = Messages.UserNotFoundForOperation
            });

        return Json(new
        {
            success = result.Succeeded,
            message = !result.Succeeded ? Messages.DeleteUserFailed : ""
        });
    }

    [HttpGet]
    public async Task<IActionResult> AdminUsers()
    {
        var adminUsers = await _userManagementService.GetAdministratorsAsync();
        return Json(new
        {
            success = true,
            data = adminUsers.Select(x => new UserVM
            {
                Id = x.Id,
                UserName = x.UserName,
                Team = x.Team
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> AllUsers()
    {
        var users = await _userManagementService.GetAllActiveAsync();

        return Json(new
        {
            success = true,
            data = users.Select(x => new UserVM
            {
                Id = x.Id,
                UserName = x.UserName,
                Team = x.Team
            })
        });
    }

    private static UserVM ToViewModel(UserDetails details)
    {
        return new UserVM
        {
            Id = details.User.Id,
            UserName = details.User.UserName,
            Team = details.User.Team,
            UserRoleIds = details.RoleIds.ToList(),
            UserRoleNames = details.RoleNames.ToList()
        };
    }
}
