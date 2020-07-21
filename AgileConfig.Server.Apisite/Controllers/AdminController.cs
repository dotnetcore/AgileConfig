using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public async Task<IActionResult> Login()
        {
            if ((await HttpContext.AuthenticateAsync()).Succeeded)
            {
                return Redirect("/");
            }

            if (!await _settingService.HasAdminPassword())
            {
                return Redirect("InitPassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm]string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "密码不能为空";
                return View();
            }

            var result = await _settingService.ValidateAdminPassword(password);
            if (result)
            {
                var claims = new List<Claim>
                {
                  new Claim("UserName","Administrator")
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = <bool>,
                    // Refreshing the authentication session should be allowed.

                    //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    // The time at which the authentication ticket expires. A 
                    // value set here overrides the ExpireTimeSpan option of 
                    // CookieAuthenticationOptions set with AddCookie.

                    //IsPersistent = true,
                    // Whether the authentication session is persisted across 
                    // multiple requests. Required when setting the 
                    // ExpireTimeSpan option of CookieAuthenticationOptions 
                    // set with AddCookie. Also required when setting 
                    // ExpiresUtc.

                    //IssuedUtc = <DateTimeOffset>,
                    // The time at which the authentication ticket was issued.

                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http 
                    // redirect response value.
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                //addlog
                await _sysLogService.AddSysLogAsync(new Data.Entity.SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = Data.Entity.SysLogType.Normal,
                    LogText = $"管理员登录成功"
                });

                return Redirect("/");
            }

            ViewBag.ErrorMessage = "登录失败：密码不正确";
            return View();
        }

        /// <summary>
        /// 初始化密码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> InitPassword()
        {
            var has = await _settingService.HasAdminPassword();
            if (has)
            {
                return Redirect("login");
            }

            return View();
        }

        [HttpGet]
        public IActionResult InitPasswordSuccess()
        {
            return View();
        }

        /// <summary>
        /// 初始化密码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> InitPassword([FromForm]string password, [FromForm]string confirmPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ErrorMessage = "密码不能为空";
                return View();
            }

            if (password.Length > 50 || confirmPassword.Length > 50)
            {
                ViewBag.ErrorMessage = "密码最长不能超过50位";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "输入的两次密码不一致";
                return View();
            }

            if (await _settingService.HasAdminPassword())
            {
                ViewBag.ErrorMessage = "密码已经设置过，不需要再次设置";
                return View();
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

                return Redirect("InitPasswordSuccess");
            }
            else
            {
                ViewBag.ErrorMessage = "初始化密码失败";
                return View();
            }
        }

        public async Task<IActionResult> Logoff()
        {
            await HttpContext.SignOutAsync();

            return Redirect("Login");
        }
    }
}
