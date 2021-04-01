using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly ISysLogService _sysLogService;
        public AdminController(ISettingService settingService, ISysLogService sysLogService)
        {
            _settingService = settingService;
            _sysLogService = sysLogService;
        }


        [HttpPost("admin/jwt/login")]
        public async Task<IActionResult> Login4AntdPro([FromBody] LoginVM model)
        {
            string password = model.password;
            if (string.IsNullOrEmpty(password))
            {
                return Json(new
                {
                    status = "error",
                    message = "密码不能为空"
                });
            }

            var result = await _settingService.ValidateAdminPassword(password);
            if (result)
            {

                var jwt = JWT.GetToken();

                //addlog
                await _sysLogService.AddSysLogAsync(new Data.Entity.SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = Data.Entity.SysLogType.Normal,
                    LogText = $"管理员登录成功"
                });

                return Json(new
                {
                    status = "ok",
                    token = jwt,
                    type = "Bearer",
                    currentAuthority = "admin"
                });
            }

            return Json(new
            {
                status = "error",
                message = "密码错误"
            });
        }

        /// <summary>
        /// is password inited ?
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PasswordInited()
        {
            var has = await _settingService.HasAdminPassword();
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

            if (await _settingService.HasAdminPassword())
            {
                return Json(new
                {
                    message = "密码已经设置过，不需要再次设置",
                    success = false
                });
            }

            var result = await _settingService.SetAdminPassword(password);

            if (result)
            {
                await _sysLogService.AddSysLogAsync(new Data.Entity.SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = Data.Entity.SysLogType.Normal,
                    LogText = $"管理员密码初始化成功"
                });

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
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordVM model)
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

            var validOld = await _settingService.ValidateAdminPassword(oldPassword);

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

            var result = await _settingService.SetAdminPassword(password);

            if (result)
            {
                await _sysLogService.AddSysLogAsync(new Data.Entity.SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = Data.Entity.SysLogType.Normal,
                    LogText = $"修改管理员密码成功"
                });

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
