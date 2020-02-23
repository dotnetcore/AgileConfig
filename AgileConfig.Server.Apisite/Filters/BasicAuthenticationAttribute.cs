using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgileConfig.Server.Apisite.Filters
{
    public class BasicAuthenticationAttribute : ActionFilterAttribute
    {
        private readonly IAppService _appService;
        public BasicAuthenticationAttribute(IAppService appService)
        {
            _appService = appService;
        }
        public async override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!await Valid(context.HttpContext.Request))
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new ContentResult();
            }
        }

        public async Task<bool> Valid(HttpRequest httpRequest)
        {
            var appid = httpRequest.Headers["appid"];
            if (string.IsNullOrEmpty(appid))
            {
                return false;
            }
            var app = await _appService.GetAsync(appid);
            if (app == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(app.Secret))
            {
                //如果没有设置secret则直接通过
                return true;
            }
            var authorization = httpRequest.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                return false;
            }

            if (!app.Enabled)
            {
                return false;
            }
            var sec = app.Secret;

            var txt = $"{appid}:{sec}";
            var data = Encoding.UTF8.GetBytes(txt);
            var auth = "Basic " + Convert.ToBase64String(data);

            return auth == authorization;
        }
    }
}
