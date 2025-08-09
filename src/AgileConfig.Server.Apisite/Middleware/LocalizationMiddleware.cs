using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Middleware
{
    /// <summary>
    /// 本地化中间件，用于根据请求头自动设置语言
    /// </summary>
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate _next;

        public LocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 尝试从查询字符串获取语言设置
            var cultureQuery = context.Request.Query["culture"];
            if (!string.IsNullOrEmpty(cultureQuery))
            {
                var culture = new CultureInfo(cultureQuery);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            // 尝试从Accept-Language头获取语言设置
            else if (context.Request.Headers.ContainsKey("Accept-Language"))
            {
                var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
                if (!string.IsNullOrEmpty(acceptLanguage))
                {
                    // 简单的语言检测逻辑
                    if (acceptLanguage.Contains("zh"))
                    {
                        var culture = new CultureInfo("zh-CN");
                        CultureInfo.CurrentCulture = culture;
                        CultureInfo.CurrentUICulture = culture;
                    }
                    else if (acceptLanguage.Contains("en"))
                    {
                        var culture = new CultureInfo("en-US");
                        CultureInfo.CurrentCulture = culture;
                        CultureInfo.CurrentUICulture = culture;
                    }
                }
            }

            await _next(context);
        }
    }
}
