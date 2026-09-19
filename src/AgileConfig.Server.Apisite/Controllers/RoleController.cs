using System;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Roles;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
public class RoleController : Controller
{
    private readonly IRoleManagementService _roleManagementService;

    public RoleController(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var roles = await _roleManagementService.GetAllAsync();

        return Json(new
        {
            success = true,
            data = roles.Select(ToViewModel).ToList()
        });
    }

    [HttpGet]
    public IActionResult SupportedPermissions()
    {
        return Json(new
        {
            success = true,
            data = _roleManagementService.GetSupportedPermissions()
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Role_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] RoleVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var result = await _roleManagementService.CreateAsync(new CreateRoleCommand(
            model.Id,
            model.Name,
            model.Description,
            model.Functions));

        return Json(new { success = result.Succeeded });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Role_Edit })]
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] RoleVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var result = await _roleManagementService.UpdateAsync(new UpdateRoleCommand(
            model.Id,
            model.Name,
            model.Description,
            model.Functions));
        if (result.Error == ApplicationError.Forbidden)
            return Json(new
            {
                success = false,
                message = "SuperAdministrator role cannot be edited"
            });

        return Json(new
        {
            success = result.Succeeded,
            message = result.Succeeded ? string.Empty : Messages.UpdateRoleFailed
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Role_Delete })]
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

        var result = await _roleManagementService.DeleteAsync(id);
        if (result.Error == ApplicationError.Forbidden && id == SystemRoleConstants.SuperAdminId)
            return Json(new
            {
                success = false,
                message = "SuperAdministrator role cannot be deleted"
            });

        return Json(new
        {
            success = result.Succeeded,
            message = result.Succeeded ? string.Empty : Messages.DeleteRoleFailed
        });
    }

    private static RoleVM ToViewModel(RoleDetails details)
    {
        return new RoleVM
        {
            Id = details.Role.Id,
            Name = details.Role.Name,
            Description = details.Role.Description,
            IsSystem = details.Role.IsSystem,
            Functions = details.Functions.ToList()
        };
    }
}
