using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgileConfig.Server.Apisite.Filters;

/// <summary>
///     Action filter that enforces basic authentication for application clients.
/// </summary>
public class AppBasicAuthenticationAttribute : ActionFilterAttribute
{
    private readonly IAppBasicAuthService _appBasicAuthService;

    public AppBasicAuthenticationAttribute(IAppBasicAuthService appBasicAuthService)
    {
        _appBasicAuthService = appBasicAuthService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!await Valid(context.HttpContext.Request))
        {
            context.HttpContext.Response.StatusCode = 403;
            context.Result = new ContentResult();
        }

        await base.OnActionExecutionAsync(context, next);
    }

    public async Task<bool> Valid(HttpRequest httpRequest)
    {
        return await _appBasicAuthService.ValidAsync(httpRequest);
    }
}