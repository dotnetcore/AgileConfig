using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Common
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.
                CreateLogger<ExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(
                        ex,
                        $"When {context.Connection.RemoteIpAddress} request {context.Request.Path} error , but not handled .\r\n {ex.Message} \r\n {ex.StackTrace}",
                        ""
                        );

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "text/html";

                    await context.Response.WriteAsync($"{(int)HttpStatusCode.InternalServerError} InternalServerError").ConfigureAwait(false);
                }
                catch (Exception ex2)
                {
                    _logger.LogError(
                        0, ex2,
                        "An exception was thrown attempting " +
                        "to execute the error handler.");
                }

                // Otherwise this handler will
                // re -throw the original exception
                throw;
            }
        }
    }
}