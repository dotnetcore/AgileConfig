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
        public async Task<IActionResult> Search(string userName,string team, int current = 1, int pageSize = 20)
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
            users = users.Where(x => x.Status == UserStatus.Normal).ToList();
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

            return Json(new
            {
                current,
                pageSize,
                success = true,
                total = total,
                data = pageList
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody]UserVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
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

            return Json(new
            {
                data = user,
                success = result,
                message = !result ? "添加用户失败，请查看错误日志" : ""
            });
        }

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

            return Json(new
            {
                data = user,
                success = result,
                message = !result ? "修改用户失败，请查看错误日志" : ""
            });
        }

        const string DefaultPassword = "123456";

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

            return Json(new
            {
                success = result,
                message = !result ? "重置用户密码失败，请查看错误日志" : ""
            });
        }

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

            return Json(new
            {
                success = result,
                message = !result ? "删除用户失败，请查看错误日志" : ""
            });
        }
    }
}
