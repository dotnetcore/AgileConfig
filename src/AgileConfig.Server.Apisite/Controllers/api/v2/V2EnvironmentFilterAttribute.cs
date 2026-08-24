using System;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgileConfig.Server.Apisite.Controllers.api.v2;

public sealed class V2EnvironmentFilterAttribute : IAsyncActionFilter
{
    private readonly ISettingService _settingService;

    public V2EnvironmentFilterAttribute(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.ActionArguments.TryGetValue("environment", out var value);
        var environment = value as string;
        var configuredEnvironment = (await _settingService.GetEnvironmentList())
            .FirstOrDefault(x => string.Equals(x, environment, StringComparison.OrdinalIgnoreCase));

        if (configuredEnvironment == null)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Unknown environment.",
                Detail = $"The environment '{environment}' is not configured."
            };
            var result = new ObjectResult(problem) { StatusCode = problem.Status };
            result.ContentTypes.Add("application/problem+json");
            context.Result = result;
            return;
        }

        context.ActionArguments["environment"] = configuredEnvironment;
        await next();
    }
}
