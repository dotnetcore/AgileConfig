using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgileConfig.Server.Apisite.Filters
{
    /// <summary>
    /// 对请求进行basic认证的filter
    /// </summary>
    public class AdmBasicAuthenticationAttribute : ActionFilterAttribute
    {
        private readonly IAdmBasicAuthService _admBasicAuthService;
        public AdmBasicAuthenticationAttribute(IAdmBasicAuthService admBasicAuthService)
        {
            _admBasicAuthService = admBasicAuthService;
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

        private async Task<bool> Valid(HttpRequest httpRequest)
        {
            return await _admBasicAuthService.ValidAsync(httpRequest);
        }
    }
}
