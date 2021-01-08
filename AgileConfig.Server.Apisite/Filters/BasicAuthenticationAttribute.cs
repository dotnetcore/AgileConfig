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
    /// <summary>
    /// 对请求进行basic认证的filter
    /// </summary>
    public class BasicAuthenticationAttribute : ActionFilterAttribute
    {
        private readonly IAppService _appService;
        public BasicAuthenticationAttribute(IAppService appService)
        {
            _appService = appService;
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
            var authorization = httpRequest.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                return false;
            }
            var authStr = authorization.First();
            //去掉basic_
            if (!authStr.StartsWith("Basic "))
            {
                return false;
            }
            authStr = authStr.Substring(6, authStr.Length - 6);
            byte[] base64Decode = null;
            try
            {
                base64Decode = Convert.FromBase64String(authStr);
            }
            catch  
            {
                return false;
            }
            var base64Str = Encoding.UTF8.GetString(base64Decode);

            if (string.IsNullOrEmpty(base64Str))
            {
                return false;
            }

            var appId = "";
            var sec = "";

           
            var baseAuthArr = base64Str.Split(':');

            if (baseAuthArr.Length>0)
            {
                appId = baseAuthArr[0];
            }
            var app = await _appService.GetAsync(appId);
            if (app == null)
            {
                return false;
            }
            if (!app.Enabled)
            {
                return false;
            }
            if (baseAuthArr.Length > 1)
            {
                sec = baseAuthArr[1];
            }

            var txt = $"{app.Id}:{app.Secret}";

            return txt == $"{appId}:{sec}";
        }
    }
}
