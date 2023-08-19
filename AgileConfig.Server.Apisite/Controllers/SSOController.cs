using AgileConfig.Server.OIDC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class SSOController : Controller
    {
        private readonly IOidcClient _oidcClient;

        public SSOController(IOidcClient oidcClient)
        {
            _oidcClient = oidcClient;
        }

        /// <summary>
        /// pass the oidc code to frontend
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Index(string code)
        {
            if (!Appsettings.IsAdminConsoleMode)
            {
                return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");
            }

            if (!Appsettings.SsoEnabled)
            {
                return BadRequest("SSO not enabled");
            }

            return Redirect(Request.PathBase + "/ui#/oidc/login?code=" + code);
        }

        public IActionResult Login()
        {
            if (!Appsettings.SsoEnabled)
            {
                return BadRequest("SSO not enabled");
            }

            var url = _oidcClient.GetAuthorizeUrl();

            
            return Redirect(url);
        }

        public IActionResult LoginUrl()
        {
            if (!Appsettings.SsoEnabled)
            {
                return BadRequest("SSO not enabled");
            }

            var url = _oidcClient.GetAuthorizeUrl();

            return Json(new
            {
                success = true,
                data = url
            });
        }
    }
}
