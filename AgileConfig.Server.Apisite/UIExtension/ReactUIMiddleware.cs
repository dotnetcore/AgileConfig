using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace AgileConfig.Server.Apisite.UIExtension
{
    public class ReactUIMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public ReactUIMiddleware(
           RequestDelegate next,
           ILoggerFactory loggerFactory
       )
        {
            _next = next;
            _logger = loggerFactory.
                CreateLogger<ReactUIMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            //handle /ui request
            if (context.Request.Path.Equals("ui", StringComparison.OrdinalIgnoreCase))
            {
                var html = await File.ReadAllTextAsync("wwwroot/index.html");

                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html, Encoding.UTF8, System.Threading.CancellationToken.None);
                return;
            }

            //handle static files that Referer = xxx/ui
            if (context.Request.Path.Value.Contains("."))
            {
                context.Request.Headers.TryGetValue("Referer", out StringValues refererValues);
                if (refererValues.Any())
                {
                    var refererValue = refererValues.First();
                    if (refererValue.EndsWith("/ui", StringComparison.OrdinalIgnoreCase))
                    {
                        //to do read static files
                    }
                }
            }

            await _next(context);
        }
    }
}
