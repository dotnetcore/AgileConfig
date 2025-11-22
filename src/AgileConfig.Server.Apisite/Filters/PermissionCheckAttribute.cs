using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgileConfig.Server.Apisite.Filters;

public class PermissionCheckAttribute : ActionFilterAttribute
{
    private readonly IConfigService _configService;
    private readonly string _functionKey;

    private readonly IPermissionService _permissionService;

    public PermissionCheckAttribute(IPermissionService permissionService, IConfigService configService, string functionKey)
    {
        _permissionService = permissionService;
        _configService = configService;

        _functionKey = functionKey;
    }

    private static string GetEnvFromArgs(IDictionary<string, object> args, IConfigService configService)
    {
        args.TryGetValue("env", out var envArg);
        var env = "";
        if (envArg == null)
            ISettingService.IfEnvEmptySetDefault(ref env);
        else
            env = envArg.ToString();

        return env;
    }

    protected virtual Task<string> GetUserId(ActionExecutingContext context)
    {
        var userId = context.HttpContext.GetUserIdFromClaim();

        return Task.FromResult(userId);
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = await GetUserId(context);
        if (string.IsNullOrEmpty(userId))
        {
            //no permission
            context.HttpContext.Response.StatusCode = 403;
            context.Result = new ContentResult();
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var userFunctions = await _permissionService.GetUserPermission(userId);

        if (userFunctions.Contains(_functionKey))
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        //no permission
        context.HttpContext.Response.StatusCode = 403;
        context.Result = new ContentResult();
        await base.OnActionExecutionAsync(context, next);
    }
}
