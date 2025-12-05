using System;
using AgileConfig.Server.OIDC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

public class SsoController : Controller
{
    private readonly IOidcClient _oidcClient;

    public SsoController(IOidcClient oidcClient)
    {
        _oidcClient = oidcClient;
    }

    /// <summary>
    ///     pass the oidc code to frontend
    /// </summary>
    /// <param name="code">Authorization code returned by the OIDC provider.</param>
    /// <returns>Redirect to the admin UI with the provided code.</returns>
    [AllowAnonymous]
    public IActionResult Index(string code)
    {
        if (!Appsettings.IsAdminConsoleMode)
            return Content($"AgileConfig Node is running now , {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} .");

        if (!Appsettings.SsoEnabled) return BadRequest("SSO not enabled");

        return Redirect(Request.PathBase + "/ui#/oidc/login?code=" + code);
    }

    public IActionResult Login()
    {
        if (!Appsettings.SsoEnabled) return BadRequest("SSO not enabled");

        var url = _oidcClient.GetAuthorizeUrl();


        return Redirect(url);
    }

    public IActionResult LoginUrl()
    {
        if (!Appsettings.SsoEnabled) return BadRequest("SSO not enabled");

        var url = _oidcClient.GetAuthorizeUrl();

        return Json(new
        {
            success = true,
            data = url
        });
    }
}