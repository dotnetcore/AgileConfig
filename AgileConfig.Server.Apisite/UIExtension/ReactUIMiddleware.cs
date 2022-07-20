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
        private static readonly ConcurrentDictionary<string, UIFileBag> StaticFilesCache = new ();
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
                    if (refererValue.EndsWith("/ui", StringComparison.OrdinalIgnoreCase) 
                        || refererValue.Contains("/monaco-editor/", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 为了适配 pathbase ，index.html 注入的 js css ，需要使用相对路径，所以要去除 /
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task RewriteIndexHtml(string filePath)
        {
            var rows = await File.ReadAllLinesAsync(filePath);
            for (int i = 0; i < rows.Length; i++)
            {
                var line = rows[i];
                if (line.Contains("window.resourceBaseUrl = \"/\""))
                {
                    if (!string.IsNullOrWhiteSpace(Appsettings.PathBase))
                    {
                        line = line.Replace("/", $"{Appsettings.PathBase}/");
                        rows[i] = line;
                    }
                }
                if (line.Contains("<link rel=\"stylesheet\" href=\"/umi."))
                {
                    line = line.Replace("/umi.", "umi.");
                    rows[i] = line;
                }
                if (line.Contains("<script src=\"/umi."))
                {
                    line = line.Replace("/umi.", "umi.");
                    rows[i] = line;
                }

            }
            await File.WriteAllLinesAsync(filePath, rows);
        }

        private static readonly string UiDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot/ui");
        public async Task Invoke(HttpContext context)
        {
            //handle /ui request
            var filePath = "";
            if (ShouldHandleUIRequest(context))
            {
                filePath = UiDirectory + "/index.html";
            }
            //handle static files that Referer = xxx/ui
            if (ShouldHandleUIStaticFilesRequest(context))
            {
                filePath = UiDirectory + context.Request.Path;
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
                if (!File.Exists(filePath))
                {
                    context.Response.StatusCode = 404;
                    return;
                }
                
                if (StaticFilesCache.TryGetValue(filePath, out var uiFile))
                {
                    // cached
                }
                else
                {
                    if (filePath.EndsWith("index.html"))
                    {
                        await RewriteIndexHtml(filePath);
                    }
                    
                    var fileData = await File.ReadAllBytesAsync(filePath);  //read file bytes
                    var lastModified = File.GetLastWriteTime(filePath);
                    var extType = Path.GetExtension(filePath);
                    uiFile = new UIFileBag()
                    {
                        FilePath = filePath,
                        Data = fileData,
                        LastModified = lastModified,
                        ExtType = extType
                    };
                    
                    StaticFilesCache.TryAdd(filePath, uiFile);
                }
                
                //判断前端缓存的文件是否过期
                if (context.Request.Headers.TryGetValue("If-Modified-Since", out StringValues values))
                {
                    var lastModified = uiFile.LastModified;
                    if (DateTime.TryParse(values.First(), out DateTime ifModifiedSince) && ifModifiedSince >= lastModified)
                    {
                        context.Response.StatusCode = 304;
                        return;
                    }
                }
                
                context.Response.OnStarting(() =>
                {
                    context.Response.ContentType = uiFile.ContentType;
                    context.Response.Headers.Add("last-modified", uiFile.LastModified.ToString("R"));
                    return Task.CompletedTask;
                });

                await context.Response.StartAsync();
                //output the file bytes
                await context.Response.BodyWriter.WriteAsync(uiFile.Data);
            }
        }
    }
}
