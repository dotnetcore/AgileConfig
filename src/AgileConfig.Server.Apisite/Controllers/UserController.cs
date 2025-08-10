using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using System.Collections.Generic;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Event;
using AgileConfig.Server.Common.Resources;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITinyEventBus _tinyEventBus;

        public UserController(IUserService userService,
            ITinyEventBus tinyEventBus
            )
        {
            _userService = userService;
            _tinyEventBus = tinyEventBus;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string userName, string team, int current = 1, int pageSize = 20)
        {
            if (current <= 0)
            {
                throw new ArgumentException(Messages.CurrentCannotBeLessThanOneUser);
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException(Messages.PageSizeCannotBeLessThanOneUser);
            }

            var users = await _userService.GetAll();
            users = users.Where(x => x.Status == UserStatus.Normal && x.Id != SystemSettings.SuperAdminId).ToList();
            if (!string.IsNullOrEmpty(userName))
            {
                users = users.Where(x => x.UserName != null && x.UserName.Contains(userName)).ToList();
            }
            if (!string.IsNullOrEmpty(team))
            {
                users = users.Where(x => x.Team != null && x.Team.Contains(team)).ToList();
            }
            users = users.OrderByDescending(x => x.CreateTime).ToList();

            var pageList = users.Skip((current - 1) * pageSize).Take(pageSize);
            var total = users.Count;

            var vms = new List<UserVM>();
            foreach (var item in pageList)
            {
                var roles = await _userService.GetUserRolesAsync(item.Id);
                var vm = new UserVM
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    Team = item.Team,
                    UserRoles = roles,
                    UserRoleNames = roles.Select(r => r.ToDesc()).ToList()
                };
                vms.Add(vm);
            }

            return Json(new
            {
                current,
                pageSize,
                success = true,
                total = total,
                data = vms
            });
        }

        [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "User.Add", Functions.User_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UserVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var oldUsers = await _userService.GetUsersByNameAsync(model.UserName);
            if (oldUsers.Any(x=>x.Status == UserStatus.Normal))
            {
                return Json(new
                {
                    success = false,
                    message = Messages.UserAlreadyExists(model.UserName)
                });
            }

            var user = new User();
            user.Id = Guid.NewGuid().ToString("N");
            var salt = Guid.NewGuid().ToString("N");
            user.Salt = salt;
            user.Password = Encrypt.Md5(model.Password + salt);
            user.Status = UserStatus.Normal;
            user.Team = model.Team;
            user.CreateTime = DateTime.Now;
            user.UserName = model.UserName;

            var addUserResult = await _userService.AddAsync(user);
            var addUserRoleResult = await _userService.UpdateUserRolesAsync(user.Id, model.UserRoles);

            if (addUserResult)
            {
                _tinyEventBus.Fire(new AddUserSuccessful(user, this.GetCurrentUserName()));
            }

            return Json(new
            {
                success = addUserResult && addUserRoleResult,
                message = !(addUserResult && addUserRoleResult) ? Messages.AddUserFailed : ""
            });
        }

        [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "User.Edit", Functions.User_Edit })]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UserVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var user = await _userService.GetUserAsync(model.Id);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = Messages.UserNotFoundForOperation
                });
            }

            user.Team = model.Team;
            user.UpdateTime = DateTime.Now;

            var result = await _userService.UpdateAsync(user);
            var reuslt1 = await _userService.UpdateUserRolesAsync(user.Id, model.UserRoles);

            if (result)
            {
                _tinyEventBus.Fire(new EditUserSuccessful(user, this.GetCurrentUserName()));
            }

            return Json(new
            {
                success = result && reuslt1,
                message = !(result && reuslt1) ? Messages.UpdateUserFailed : ""
            });
        }

        const string DefaultPassword = "123456";

        [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "User.ResetPassword", Functions.User_Edit })]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId");
            }

            var user = await _userService.GetUserAsync(userId);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = Messages.UserNotFoundForOperation
                });
            }

            user.Password = Encrypt.Md5(DefaultPassword + user.Salt);

            var result = await _userService.UpdateAsync(user);
            if (result)
            {
                _tinyEventBus.Fire(new ResetUserPasswordSuccessful(this.GetCurrentUserName(), user.UserName));
            }

            return Json(new
            {
                success = result,
                message = !result ? Messages.ResetUserPasswordFailed : ""
            });
        }

        [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "User.Delete", Functions.User_Delete })]
        [HttpPost]
        public async Task<IActionResult> Delete(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var user = await _userService.GetUserAsync(userId);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = Messages.UserNotFoundForOperation
                });
            }

            user.Status = UserStatus.Deleted;
            var result = await _userService.UpdateAsync(user);
            if (result)
            {
                _tinyEventBus.Fire(new DeleteUserSuccessful(user, this.GetCurrentUserName()));
            }

            return Json(new
            {
                success = result,
                message = !result ? Messages.DeleteUserFailed : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> AdminUsers()
        {
            var adminUsers = await _userService.GetUsersByRoleAsync(Role.Admin);
            adminUsers = adminUsers.Where(x => x.Status == UserStatus.Normal).ToList();
            return Json(new
            {
                success = true,
                data = adminUsers.OrderBy(x=>x.Team).ThenBy(x => x.UserName).Select(x=> new UserVM { 
                    Id = x.Id,
                    UserName = x.UserName,
                    Team = x.Team
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> AllUsers()
        {
            var users = await _userService.GetAll();
            users = users.Where(x => x.Status == UserStatus.Normal && x.Id != SystemSettings.SuperAdminId).ToList();

            return Json(new
            {
                success = true,
                data = users.OrderBy(x => x.Team).ThenBy(x=>x.UserName).Select(x => new UserVM
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Team = x.Team
                })
            });
        }
    }
}
