using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        private static readonly IReadOnlyList<string> SupportedFunctions = new List<string>
        {
            Functions.App_Add,
            Functions.App_Edit,
            Functions.App_Delete,
            Functions.App_Auth,

            Functions.Config_Add,
            Functions.Config_Edit,
            Functions.Config_Delete,
            Functions.Config_Publish,
            Functions.Config_Offline,

            Functions.Node_Add,
            Functions.Node_Delete,

            Functions.Client_Disconnect,

            Functions.User_Add,
            Functions.User_Edit,
            Functions.User_Delete,

            Functions.Role_Add,
            Functions.Role_Edit,
            Functions.Role_Delete
        };

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var roles = await _roleService.GetAllAsync();
            // Filter out Super Administrator role to prevent it from being assigned through the frontend
            var vms = roles
                .Where(r => r.Id != SystemRoleConstants.SuperAdminId)
                .Select(ToViewModel)
                .OrderByDescending(r => r.IsSystem)
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
                data = SupportedFunctions
            });
        }

        [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Role.Add", Functions.Role_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RoleVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

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
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var role = new Role()
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
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await _roleService.DeleteAsync(id);
            return Json(new
            {
                success = result,
                message = result ? string.Empty : Messages.DeleteRoleFailed
            });
        }

        private static RoleVM ToViewModel(Role role)
        {
            return new RoleVM
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystem = role.IsSystem,
                Functions = ParseFunctions(role.FunctionsJson)
            };
        }

        private static List<string> ParseFunctions(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                var funcs = JsonSerializer.Deserialize<List<string>>(json);
                return funcs ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
