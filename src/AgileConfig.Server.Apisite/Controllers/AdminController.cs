using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.OIDC;
using System.Collections.Generic;
using AgileConfig.Server.Event;
using AgileConfig.Server.Common.EventBus;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly IJwtService _jwtService;
        private readonly IOidcClient _oidcClient;
        private readonly ITinyEventBus _tinyEventBus;
        private readonly ISystemInitializationService _systemInitializationService;

        public AdminController(
            ISettingService settingService,
            IUserService userService,
            IPermissionService permissionService,
            IJwtService jwtService,
            IOidcClient oidcClient,
            ITinyEventBus tinyEventBus,
            ISystemInitializationService systemInitializationService
            )
        {
            _settingService = settingService;
            _userService = userService;
            _permissionService = permissionService;
            _jwtService = jwtService;
            _oidcClient = oidcClient;
            _tinyEventBus = tinyEventBus;
            _systemInitializationService = systemInitializationService;
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
                    message = Messages.PasswordCannotBeEmpty
                });
            }

            var result = await _userService.ValidateUserPassword(userName, model.password);
            if (result)
            {
                var response = await LoginSuccessful(userName);
                return Json(response);
            }

            return Json(new
            {
                status = "error",
                message = Messages.PasswordError
            });
        }

        private async Task<object> LoginSuccessful(string userName)
        {
            var user = (await _userService.GetUsersByNameAsync(userName)).First();
            var userRoles = await _userService.GetUserRolesAsync(user.Id);
            var jwt = _jwtService.GetToken(user.Id, user.UserName,
                userRoles.Any(r => r.Id == SystemRoleConstants.AdminId || r.Id == SystemRoleConstants.SuperAdminId));
            var userFunctions = await _permissionService.GetUserPermission(user.Id);

            _tinyEventBus.Fire(new LoginEvent(user.UserName));

            return new
            {
                status = "ok",
                token = jwt,
                type = "Bearer",
                currentAuthority = userRoles.Select(r => r.Code),
                currentFunctions = userFunctions
            };
        }

        [HttpGet("admin/oidc/login")]
        public async Task<IActionResult> OidcLoginByCode(string code)
        {
            if (!Appsettings.SsoEnabled)
            {
                return BadRequest("SSO not enabled");
            }

            var tokens = await _oidcClient.Validate(code);

            if (string.IsNullOrEmpty(tokens.IdToken))
            {
                return Json(new
                {
                    message = "Code validate failed",
                    success = false
                });
            }

            var userInfo = _oidcClient.UnboxIdToken(tokens.IdToken);

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
                    Salt = "",
                    Source = UserSource.SSO
                };
                await _userService.AddAsync(newUser);
                await _userService.UpdateUserRolesAsync(newUser.Id, new List<string> { SystemRoleConstants.OperatorId });
            }
            else if (user.Status == UserStatus.Deleted)
            {
                return Json(new
                {
                    status = "error",
                    message = Messages.UserDeleted
                });
            }

            var response = await LoginSuccessful(userInfo.UserName);
            return Json(response);
        }

        /// <summary>
        /// is password inited ?
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult PasswordInited()
        {
            var has = _systemInitializationService.HasSa();
            return Json(new
            {
                success = true,
                data = has
            });
        }

        /// <summary>
        /// Initialize the administrator password.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult InitPassword([FromBody] InitPasswordVM model)
        {
            var password = model.password;
            var confirmPassword = model.confirmPassword;

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new
                {
                    message = Messages.PasswordCannotBeEmpty,
                    success = false
                });
            }

            if (password.Length > 50 || confirmPassword.Length > 50)
            {
                return Json(new
                {
                    message = Messages.PasswordMaxLength50,
                    success = false
                });
            }

            if (password != confirmPassword)
            {
                return Json(new
                {
                    message = Messages.PasswordMismatch,
                    success = false
                });
            }

            if ( _systemInitializationService.HasSa())
            {
                return Json(new
                {
                    message = Messages.PasswordAlreadySet,
                    success = false
                });
            }

            var result = _systemInitializationService.TryInitSaPassword(password);

            if (result)
            {
                _tinyEventBus.Fire(new InitSaPasswordSuccessful());

                return Json(new
                {
                    success = true
                });
            }
            else
            {
                return Json(new
                {
                    message = Messages.InitPasswordFailed,
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
                    message = Messages.DemoModeNoPasswordChange
                });
            }

            var password = model.password;
            var confirmPassword = model.confirmPassword;
            var oldPassword = model.oldPassword;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(oldPassword))
            {
                return Json(new
                {
                    message = Messages.OriginalPasswordCannotBeEmpty,
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
                    message = Messages.OriginalPasswordError,
                    err_code = "err_resetpassword_02",
                    success = false
                });
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new
                {
                    message = Messages.NewPasswordCannotBeEmpty,
                    err_code = "err_resetpassword_03",
                    success = false
                });
            }

            if (password.Length > 50 || confirmPassword.Length > 50)
            {
                return Json(new
                {
                    message = Messages.NewPasswordMaxLength50,
                    err_code = "err_resetpassword_04",
                    success = false
                });
            }

            if (password != confirmPassword)
            {
                return Json(new
                {
                    message = Messages.NewPasswordMismatch,
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
                    message = Messages.UserNotFound,
                    err_code = "err_resetpassword_06",
                    success = false
                });
            }

            user.Password = Encrypt.Md5(password + user.Salt);
            var result = await _userService.UpdateAsync(user);

            if (result)
            {
                _tinyEventBus.Fire(new ChangeUserPasswordSuccessful(user.UserName));

                return Json(new
                {
                    success = true
                });
            }
            else
            {
                return Json(new
                {
                    message = Messages.ChangePasswordFailed,
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
