using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Apisite.UIExtension
{
    public class ReactUIMiddleware
    {
        private static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        {
            {".html", "text/html; charset=utf-8"},
            {".css", "text/css; charset=utf-8"},
            {".js", "application/javascript"},
            {".png", "image/png"},
            {".svg", "image/svg+xml"},
            { ".json","application/json;charset=utf-8"},
            { ".ico","image/x-icon"}
        };
        private static ConcurrentDictionary<string, byte[]> _staticFilesCache = new ConcurrentDictionary<string, byte[]>();
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

        private bool IsAdminConsoleMode => "true".Equals(Global.Config["adminConsole"], StringComparison.OrdinalIgnoreCase);

        private bool ShouldHandleUIRequest(HttpContext context)
        {
            return context.Request.Path.HasValue && context.Request.Path.Value.Equals("/ui", StringComparison.OrdinalIgnoreCase);
        }

        private bool ShouldHandleUIStaticFilesRequest(HttpContext context)
        {
            //请求的的Referer为 0.0.0.0/ui ,以此为依据判断是否是reactui需要的静态文件
            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("."))
            {
                context.Request.Headers.TryGetValue("Referer", out StringValues refererValues);
                if (refererValues.Any())
                {
                    var refererValue = refererValues.First();
                    if (refererValue.EndsWith("/ui", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task Invoke(HttpContext context)
        {
            const string uiDirectory = "wwwroot/ui";
            //handle /ui request
            var filePath = "";
            if (ShouldHandleUIRequest(context))
            {
                filePath = uiDirectory + "/index.html";
            }
            //handle static files that Referer = xxx/ui
            if (ShouldHandleUIStaticFilesRequest(context))
            {
                filePath = uiDirectory + context.Request.Path;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                await _next(context);
            }
            else
            {
                if (!IsAdminConsoleMode)
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("This node is not an admin console node .");
                    return;
                }
                //output the file bytes

                if (!File.Exists(filePath))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                context.Response.OnStarting(() =>
                {
                    var extType = Path.GetExtension(filePath);
                    if (_contentTypes.TryGetValue(extType, out string contentType))
                    {
                        context.Response.ContentType = contentType;
                    }
                    return Task.CompletedTask;
                });

                await context.Response.StartAsync();

                byte[] fileData = null;
                if (_staticFilesCache.TryGetValue(filePath, out byte[] outfileData))
                {
                    fileData = outfileData;
                }
                else
                {
                    fileData = await File.ReadAllBytesAsync(filePath);
                    _staticFilesCache.TryAdd(filePath, fileData);
                }
                await context.Response.BodyWriter.WriteAsync(fileData);

                return;
            }
        }
    }
}
