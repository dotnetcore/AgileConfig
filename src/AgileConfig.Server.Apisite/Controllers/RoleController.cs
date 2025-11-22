using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
public class RoleController : Controller
{
    private readonly IRoleFunctionRepository _roleFunctionRepository;
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService, IRoleFunctionRepository roleFunctionRepository)
    {
        _roleService = roleService;
        _roleFunctionRepository = roleFunctionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var roles = await _roleService.GetAllAsync();
        // Filter out Super Administrator role to prevent it from being assigned through the frontend
        var vms = new List<RoleVM>();
        foreach (var role in roles.Where(r => r.Id != SystemRoleConstants.SuperAdminId))
            vms.Add(await ToViewModel(role));

        vms = vms.OrderByDescending(r => r.IsSystem)
            .ThenBy(r => r.Name)
            .ToList();

        return Json(new
        {
            success = true,
            data = vms
        });
    }

    [HttpGet]
    public IActionResult SupportedPermissions()
    {
        return Json(new
        {
            success = true,
            data = Functions.GetAllPermissions()
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Role.Add", Functions.Role_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] RoleVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var role = new Role
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description ?? string.Empty,
            IsSystem = false
        };

        await _roleService.CreateAsync(role, model.Functions ?? Enumerable.Empty<string>());

        return Json(new { success = true });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Role.Edit", Functions.Role_Edit })]
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] RoleVM model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        // Prevent editing SuperAdministrator role
        if (model.Id == SystemRoleConstants.SuperAdminId)
            return Json(new
            {
                success = false,
                message = "SuperAdministrator role cannot be edited"
            });

        var role = new Role
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description ?? string.Empty,
            IsSystem = model.IsSystem
        };

        var result = await _roleService.UpdateAsync(role, model.Functions ?? Enumerable.Empty<string>());

        return Json(new
        {
            success = result,
            message = result ? string.Empty : Messages.UpdateRoleFailed
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Role.Delete", Functions.Role_Delete })]
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

        // Prevent deleting SuperAdministrator role
        if (id == SystemRoleConstants.SuperAdminId)
            return Json(new
            {
                success = false,
                message = "SuperAdministrator role cannot be deleted"
            });

        var result = await _roleService.DeleteAsync(id);
        return Json(new
        {
            success = result,
            message = result ? string.Empty : Messages.DeleteRoleFailed
        });
    }

    private async Task<RoleVM> ToViewModel(Role role)
    {
        var roleFunctions = await _roleFunctionRepository.QueryAsync(x => x.RoleId == role.Id);
        return new RoleVM
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            Functions = roleFunctions.Select(rf => rf.FunctionId).ToList()
        };
    }
}