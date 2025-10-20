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
            "GLOBAL_" + Functions.App_Add,
            "GLOBAL_" + Functions.App_Edit,
            "GLOBAL_" + Functions.App_Delete,
            "GLOBAL_" + Functions.App_Auth,

            "GLOBAL_" + Functions.Config_Add,
            "GLOBAL_" + Functions.Config_Edit,
            "GLOBAL_" + Functions.Config_Delete,
            "GLOBAL_" + Functions.Config_Publish,
            "GLOBAL_" + Functions.Config_Offline,

            "GLOBAL_" + Functions.Node_Add,
            "GLOBAL_" + Functions.Node_Delete,

            "GLOBAL_" + Functions.Client_Disconnect,

            "GLOBAL_" + Functions.User_Add,
            "GLOBAL_" + Functions.User_Edit,
            "GLOBAL_" + Functions.User_Delete,

            "GLOBAL_" + Functions.Role_Add,
            "GLOBAL_" + Functions.Role_Edit,
            "GLOBAL_" + Functions.Role_Delete
        };

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var roles = await _roleService.GetAllAsync();
            var vms = roles.Select(ToViewModel).OrderByDescending(r => r.IsSystem).ThenBy(r => r.Name).ToList();

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

            try
            {
                var role = new Role
                {
                    Id = model.Id,
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description ?? string.Empty,
                    IsSystem = false
                };

                await _roleService.CreateAsync(role, model.Functions ?? Enumerable.Empty<string>());

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Role.Edit", Functions.Role_Edit })]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] RoleVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var role = new Role()
                {
                    Id = model.Id,
                    Code = model.Code,
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
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "Role.Delete", Functions.Role_Delete })]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                var result = await _roleService.DeleteAsync(id);
                return Json(new
                {
                    success = result,
                    message = result ? string.Empty : Messages.DeleteRoleFailed
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        private static RoleVM ToViewModel(Role role)
        {
            return new RoleVM
            {
                Id = role.Id,
                Code = role.Code,
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
