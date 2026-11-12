using System;
using System.Globalization;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite;

public sealed class DatabaseAvailabilityMiddleware(
    RequestDelegate next,
    InitializationState initialization,
    ILogger<DatabaseAvailabilityMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path == "/health/live")
        {
            await context.Response.WriteAsync("ok", context.RequestAborted);
            return;
        }

        if (!initialization.IsReady)
        {
            await Unavailable(context, "The node is initializing its database. Please retry later.", TimeSpan.FromSeconds(2));
            return;
        }

        try
        {
            await next(context);
        }
        catch (DatabaseUnavailableException exception) when (!context.Response.HasStarted)
        {
            logger.LogWarning("Database unavailable for {Path}; returning HTTP 503.", context.Request.Path);
            await Unavailable(context, exception.Message, exception.RetryAfter);
        }
    }

    private static async Task Unavailable(HttpContext context, string message, TimeSpan retryAfter)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
            .ToString(CultureInfo.InvariantCulture);
        await context.Response.WriteAsJsonAsync(new { message }, context.RequestAborted);
    }
}
