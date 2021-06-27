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
using AgileConfig.Server.Service;
using System.Dynamic;
using AgileConfig.Server.Apisite.Utilites;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string userName, string team, int current = 1, int pageSize = 20)
        {
            if (current <= 0)
            {
                throw new ArgumentException("current can not less than 1 .");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("pageSize can not less than 1 .");
            }

            var users = await _userService.GetAll();
            users = users.Where(x => x.Status == UserStatus.Normal && x.Id != SettingService.SuperAdminId).ToList();
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
            var totalPages = total / pageSize;
            if ((total % pageSize) > 0)
            {
                totalPages++;
            }

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

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "User.Add", Functions.User_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UserVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var oldUsers = await _userService.GetUsersByNameAsync(model.UserName);
            if (oldUsers.Any(x=>x.Status == UserStatus.Normal))
            {
                return Json(new
                {
                    success = false,
                    message = "已存在用户" + model.UserName
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

            var result = await _userService.AddAsync(user);
            var reuslt1 = await _userService.UpdateUserRolesAsync(user.Id, model.UserRoles);

            if (result)
            {
                dynamic param = new ExpandoObject();
                param.userName = this.GetCurrentUserName();
                param.user = user;
                TinyEventBus.Instance.Fire(EventKeys.ADD_USER_SUCCESS, param);
            }

            return Json(new
            {
                success = result && reuslt1,
                message = !(result && reuslt1) ? "添加用户失败，请查看错误日志" : ""
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "User.Edit", Functions.User_Edit })]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UserVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var user = await _userService.GetUserAsync(model.Id);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的用户。"
                });
            }

            user.Team = model.Team;
            user.UpdateTime = DateTime.Now;

            var result = await _userService.UpdateAsync(user);
            var reuslt1 = await _userService.UpdateUserRolesAsync(user.Id, model.UserRoles);

            if (result)
            {
                dynamic param = new ExpandoObject();
                param.userName = this.GetCurrentUserName();
                param.user = user;
                TinyEventBus.Instance.Fire(EventKeys.EDIT_USER_SUCCESS, param);
            }

            return Json(new
            {
                success = result && reuslt1,
                message = !(result && reuslt1) ? "修改用户失败，请查看错误日志" : ""
            });
        }

        const string DefaultPassword = "123456";

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "User.ResetPassword", Functions.User_Edit })]
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
                    message = "未找到对应的用户。"
                });
            }

            user.Password = Encrypt.Md5(DefaultPassword + user.Salt);

            var result = await _userService.UpdateAsync(user);
            if (result)
            {
                dynamic param = new ExpandoObject();
                param.user = user;
                param.userName = this.GetCurrentUserName();
                TinyEventBus.Instance.Fire(EventKeys.RESET_USER_PASSWORD_SUCCESS, param);
            }

            return Json(new
            {
                success = result,
                message = !result ? "重置用户密码失败，请查看错误日志" : ""
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "User.Delete", Functions.User_Delete })]
        [HttpPost]
        public async Task<IActionResult> Delete(string userId)
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
                    message = "未找到对应的用户。"
                });
            }

            user.Status = UserStatus.Deleted;
            var result = await _userService.UpdateAsync(user);
            if (result)
            {
                dynamic param = new ExpandoObject();
                param.userName = this.GetCurrentUserName();
                param.user = user;
                TinyEventBus.Instance.Fire(EventKeys.DELETE_USER_SUCCESS, param);
            }

            return Json(new
            {
                success = result,
                message = !result ? "删除用户失败，请查看错误日志" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> adminUsers()
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
            users = users.Where(x => x.Status == UserStatus.Normal && x.Id != SettingService.SuperAdminId).ToList();

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
