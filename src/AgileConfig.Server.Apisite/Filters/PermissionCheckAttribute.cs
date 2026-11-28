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
    private readonly AppScope _appScope;
    private readonly IConfigService _configService;
    private readonly string _functionKey;

    private readonly IPermissionService _permissionService;

    public PermissionCheckAttribute(IPermissionService permissionService, IConfigService configService,
        string functionKey, AppScope appScope = AppScope.None)
    {
        _permissionService = permissionService;
        _configService = configService;

        _functionKey = functionKey;
        _appScope = appScope;
    }

    private static string GetEnvFromArgs(IDictionary<string, object> args)
    {
        if (args.TryGetValue("env", out var env))
            return env switch
            {
                EnvString envString => envString.Value,
                string text => text,
                _ => null
            };

        return args.TryGetValue("environment", out var environment) ? environment as string : null;
    }

    private bool IsAppIdArgument(string name)
    {
        return name is "appId" or "applicationId" || (_appScope == AppScope.App && name == "id");
    }

    private bool IsLookupIdArgument(string name)
    {
        return _appScope switch
        {
            AppScope.Config => name is "id" or "configId",
            AppScope.PublishTimeline => name is "publishTimelineId" or "historyId",
            _ => false
        };
    }

    /// <summary>
    ///     Collect the ids of every application the action is about to touch.
    /// </summary>
    /// <returns>The application ids, or null when the request does not name a usable target.</returns>
    private async Task<List<string>> GetTargetAppIds(IDictionary<string, object> args)
    {
        var appIds = new List<string>();
        // Configuration ids or publish timeline ids, depending on the scope.
        var lookupIds = new List<string>();

        foreach (var (name, value) in args)
            switch (value)
            {
                case string text:
                    if (IsAppIdArgument(name)) appIds.Add(text);
                    else if (IsLookupIdArgument(name)) lookupIds.Add(text);
                    break;
                case IEnumerable<string> texts when _appScope == AppScope.Config && name == "ids":
                    lookupIds.AddRange(texts);
                    break;
                case IAppIdModel model:
                    appIds.Add(model.AppId);
                    // An edit names the target app in the body, but the stored configuration may belong to another one.
                    if (_appScope == AppScope.Config && model is ConfigVM { Id.Length: > 0 } config)
                        lookupIds.Add(config.Id);
                    break;
                case IEnumerable<IAppIdModel> models:
                    appIds.AddRange(models.Select(x => x?.AppId));
                    break;
                case IAppModel app when _appScope == AppScope.App:
                    appIds.Add(app.Id);
                    break;
            }

        if (appIds.Count == 0 && lookupIds.Count == 0) return null;
        if (lookupIds.Count == 0) return appIds;

        var env = GetEnvFromArgs(args);
        if (string.IsNullOrEmpty(env) || lookupIds.Any(string.IsNullOrEmpty)) return null;

        // Ids that do not exist are left to the action, which reports them as not found.
        foreach (var id in lookupIds)
            if (_appScope == AppScope.Config)
            {
                var config = await _configService.GetAsync(id, env);
                if (config != null) appIds.Add(config.AppId);
            }
            else
            {
                var timeline = await _configService.GetPublishTimeLineNodeAsync(id, env);
                if (timeline != null) appIds.Add(timeline.AppId);
            }

        return appIds;
    }

    private async Task<bool> CanAccessTargetApps(ActionExecutingContext context, string userId)
    {
        if (_appScope == AppScope.None) return true;

        var access = await _permissionService.GetUserAppAccessAsync(userId);
        context.HttpContext.SetUserAppAccess(access);
        if (access.IsAdmin || _appScope == AppScope.InAction) return true;

        var appIds = await GetTargetAppIds(context.ActionArguments);

        return appIds != null && appIds.All(access.CanAccess);
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

        if (userFunctions.Contains(_functionKey) && await CanAccessTargetApps(context, userId))
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