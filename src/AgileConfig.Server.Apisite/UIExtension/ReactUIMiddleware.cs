using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using AgileConfig.Server.Common;
using Microsoft.AspNetCore.Hosting;

namespace AgileConfig.Server.Apisite.UIExtension
{
    public class ReactUiMiddleware
    {
        private static string _uiDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot/ui");
        private static readonly ConcurrentDictionary<string, UiFileBag> StaticFilesCache = new();
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ReactUiMiddleware(
           RequestDelegate next,
           ILoggerFactory loggerFactory,
           IWebHostEnvironment environment
       )
        {
            _next = next;
            _logger = loggerFactory.
                CreateLogger<ReactUiMiddleware>();
#if DEBUG
            //if debug mode, try to switch to project wwwwroot dir
            var projectUiPath = Path.Combine(environment.ContentRootPath, "wwwroot/ui");
            if (Directory.Exists(projectUiPath))
            {
                _uiDirectory = projectUiPath;
            }
#endif
        }

        private bool IsAdminConsoleMode => "true".Equals(Global.Config["adminConsole"], StringComparison.OrdinalIgnoreCase);

        private static bool ShouldHandleUiRequest(HttpContext context)
        {
            return context.Request.Path is { HasValue: true, Value: not null } && context.Request.Path.Value.Equals("/ui", StringComparison.OrdinalIgnoreCase);
        }

        private bool ShouldHandleUiStaticFilesRequest(HttpContext context)
        {
            // Treat requests whose referer is 0.0.0.0/ui as static assets required by the React UI.
            if (context.Request.Path is { HasValue: true, Value: not null } && context.Request.Path.Value.Contains('.'))
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
        /// To support path base, make injected JS and CSS in index.html use relative paths by removing the leading slash.
        /// </summary>
        /// <param name="filePath">Absolute path to the index.html file to rewrite.</param>
        /// <returns>Task that completes when the file has been rewritten.</returns>
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

        public async Task Invoke(HttpContext context)
        {
            //handle /ui request
            var filePath = "";
            if (ShouldHandleUiRequest(context))
            {
                filePath = _uiDirectory + "/index.html";
            }
            //handle static files that Referer = xxx/ui
            if (ShouldHandleUiStaticFilesRequest(context))
            {
                filePath = _uiDirectory + context.Request.Path;
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
                    var fileData = await File.ReadAllBytesAsync(filePath);  // read file bytes
                    var lastModified = File.GetLastWriteTime(filePath);
                    var extType = Path.GetExtension(filePath);
                    uiFile = new UiFileBag()
                    {
                        FilePath = filePath,
                        Data = fileData,
                        LastModified = lastModified,
                        ExtType = extType
                    };

                    StaticFilesCache.TryAdd(filePath, uiFile);
                }

                // Check whether the cached file on the client has expired.
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
                    context.Response.Headers.TryAdd("last-modified", uiFile.LastModified.ToString("R"));
                    return Task.CompletedTask;
                });

                await context.Response.StartAsync();
                //output the file bytes
                await context.Response.BodyWriter.WriteAsync(uiFile.Data);
            }
        }
    }
}
