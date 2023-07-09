using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Expressions;
using System.Dynamic;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Service;
using AgileConfig.Server.OIDC;
using FreeSql.Internal.ObjectPool;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly IPremissionService _permissionService;
        private readonly IJwtService _jwtService;
        public AdminController(
            ISettingService settingService, 
            IUserService userService,
            IPremissionService permissionService,
            IJwtService jwtService)
        {
            _settingService = settingService;
            _userService = userService;
            _permissionService = permissionService;
            _jwtService = jwtService;
        }


        [HttpPost("admin/jwt/login")]
        public async Task<IActionResult> Login4AntdPro([FromBody] LoginVM model)
        {
            string userName = model.userName;
            string password = model.password;
            if (string.IsNullOrEmpty(password))
            {
                return Json(new
                {
                    status = "error",
                    message = "密码不能为空"
                });
            }

            var result = await _userService.ValidateUserPassword(userName, model.password);
            if (result)
            {
                var response =  await LoginSuccessful(userName);
                return Json(response);
            }

            return Json(new
            {
                status = "error",
                message = "密码错误"
            });
        }

        private async Task<object> LoginSuccessful(string userName)
        {
            var user = (await _userService.GetUsersByNameAsync(userName)).First();
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var jwt = _jwtService.GetToken(user.Id, user.UserName, userRoles.Any(r => r == Role.Admin || r == Role.SuperAdmin));
            var userFunctions = await _permissionService.GetUserPermission(user.Id);

            dynamic param = new ExpandoObject();
            param.userName = user.UserName;
            TinyEventBus.Instance.Fire(EventKeys.USER_LOGIN_SUCCESS, param);

            return new
            {
                status = "ok",
                token = jwt,
                type = "Bearer",
                currentAuthority = userRoles.Select(r => r.ToString()),
                currentFunctions = userFunctions
            };
        }

        [HttpGet("admin/oidc")]
        public async Task<IActionResult> Oidc(string code)
        {
            var oidcSetting = new ConfigfileOidcSettingProvider().GetSetting();
            var client = new OidcClient(oidcSetting);

            var tokens = await client.Validate(code);

            if (string.IsNullOrEmpty(tokens.IdToken))
            {
                return Json(new
                {
                    message = "Code validate failed",
                    success = false
                });
            }

            var userInfo = client.UnboxIdToken(tokens.IdToken);

            if (string.IsNullOrEmpty(userInfo.Id) || string.IsNullOrEmpty(userInfo.UserName))
            {
                return Json(new
                {
                    message = "IdToken invalid",
                    success = false
                });
            }

            var user = (await _userService.GetUsersByNameAsync((string)userInfo.UserName)).FirstOrDefault();

            if (user == null)
            {
                //user first login to system, should be inserted into the DB
                var newUser = new User()
                {
                    Id = userInfo.Id,
                    UserName = userInfo.UserName,
                    Password = "/",
                    CreateTime = DateTime.Now,
                    Status = UserStatus.Normal,
                    Salt = ""
                };
                await _userService.AddAsync(newUser);
            }

            var response = await LoginSuccessful(userInfo.UserName);
            return Json(response);
        }

        /// <summary>
        /// is password inited ?
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PasswordInited()
        {
            var has = await _settingService.HasSuperAdmin();
            return Json(new
            {
                success = true,
                data = has
            });
        }

        /// <summary>
        /// 初始化密码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> InitPassword([FromBody] InitPasswordVM model)
        {
            var password = model.password;
            var confirmPassword = model.confirmPassword;

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new
                {
                    message = "密码不能为空",
                    success = false
                });
            }

            if (password.Length > 50 || confirmPassword.Length > 50)
            {
                return Json(new
                {
                    message = "密码最长不能超过50位",
                    success = false
                });
            }

            if (password != confirmPassword)
            {
                return Json(new
                {
                    message = "输入的两次密码不一致",
                    success = false
                });
            }

            if (await _settingService.HasSuperAdmin())
            {
                return Json(new
                {
                    message = "密码已经设置过，不需要再次设置",
                    success = false
                });
            }

            var result = await _settingService.SetSuperAdminPassword(password);

            if (result)
            {
                TinyEventBus.Instance.Fire(EventKeys.INIT_SUPERADMIN_PASSWORD_SUCCESS);

                return Json(new
                {
                    success = true
                });
            }
            else
            {
                return Json(new
                {
                    message = "初始化密码失败",
                    success = false
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVM model)
        {
            if (Appsettings.IsPreviewMode)
            {
                return Json(new
                {
                    success = false,
                    message = "演示模式请勿修改管理密码"
                });
            }

            var password = model.password;
            var confirmPassword = model.confirmPassword;
            var oldPassword = model.oldPassword;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(oldPassword))
            {
                return Json(new
                {
                    message = "原始密码不能为空",
                    err_code = "err_resetpassword_01",
                    success = false
                });
            }

            var userName = this.GetCurrentUserName();
            var validOld = await _userService.ValidateUserPassword(userName, oldPassword);

            if (!validOld)
            {
                return Json(new
                {
                    message = "原始密码错误，请重新再试",
                    err_code = "err_resetpassword_02",
                    success = false
                });
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new
                {
                    message = "新密码不能为空",
                    err_code = "err_resetpassword_03",
                    success = false
                });
            }

            if (password.Length > 50 || confirmPassword.Length > 50)
            {
                return Json(new
                {
                    message = "新密码最长不能超过50位",
                    err_code = "err_resetpassword_04",
                    success = false
                });
            }

            if (password != confirmPassword)
            {
                return Json(new
                {
                    message = "输入的两次新密码不一致",
                    err_code = "err_resetpassword_05",
                    success = false
                });
            }

            var users = await _userService.GetUsersByNameAsync(this.GetCurrentUserName());
            var user = users.Where(x => x.Status == UserStatus.Normal).FirstOrDefault();

            if (user == null)
            {
                return Json(new
                {
                    message = "未找到对应的用户",
                    err_code = "err_resetpassword_06",
                    success = false
                });
            }

            user.Password = Encrypt.Md5(password + user.Salt);
            var result = await _userService.UpdateAsync(user);

            if (result)
            {
                dynamic param = new ExpandoObject();
                param.userName = user.UserName;
                TinyEventBus.Instance.Fire(EventKeys.CHANGE_USER_PASSWORD_SUCCESS, param);

                return Json(new
                {
                    success = true
                });
            }
            else
            {
                return Json(new
                {
                    message = "修改密码失败",
                    success = false
                });
            }
        }

        public async Task<IActionResult> Logoff()
        {
            await HttpContext.SignOutAsync();

            return Redirect("Login");
        }
    }
}
