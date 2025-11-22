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
    protected const string AppMatchPatten = "APP_{0}_{1}";

    /// <summary>
    ///     Static mapping of permission keys to delegates that extract the appId from action parameters.
    ///     Attributes cannot accept delegates directly, so the functions are predefined here.
    /// </summary>
    protected static readonly
        FrozenDictionary<string, Func<ActionExecutingContext, IPermissionService, IConfigService, string>>
        GetAppIdParamFuncs =
            new Dictionary<string, Func<ActionExecutingContext, IPermissionService, IConfigService, string>>
            {
                {
                    "Config.Add", (args, premission, config) =>
                    {
                        var model = args.ActionArguments["model"];
                        return (model as IAppIdModel)?.AppId;
                    }
                },
                {
                    "Config.AddRange", (args, permission, config) =>
                    {
                        var model = args.ActionArguments["model"];
                        return (model as List<ConfigVM>)?.FirstOrDefault()?.AppId;
                    }
                },
                {
                    "Config.EnvSync", (args, permission, config) =>
                    {
                        var appId = args.ActionArguments["appId"];
                        return appId?.ToString();
                    }
                },
                {
                    "Config.Edit", (args, permission, config) =>
                    {
                        var model = args.ActionArguments["model"];
                        return (model as IAppIdModel)?.AppId;
                    }
                },
                {
                    "Config.Delete", (args, permission, configService) =>
                    {
                        var id = args.ActionArguments["id"];
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(id.ToString(), env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
                },
                {
                    "Config.DeleteSome", (args, permission, configService) =>
                    {
                        var ids = args.ActionArguments["ids"] as List<string>;
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(ids.FirstOrDefault(), env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
                },
                {
                    "Config.Offline", (args, permission, configService) =>
                    {
                        var id = args.ActionArguments["configId"];
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(id.ToString(), env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
                },
                {
                    "Config.OfflineSome", (args, permission, configService) =>
                    {
                        var ids = args.ActionArguments["configIds"] as List<string>;
                        var id = ids?.FirstOrDefault();
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(ids.FirstOrDefault(), env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
                },
                {
                    "Config.Publish", (args, permission, configService) =>
                    {
                        var model = args.ActionArguments["model"] as IAppIdModel;

                        return model?.AppId;
                    }
                },
                {
                    "Config.Publish_API", (args, permission, configService) =>
                    {
                        var appId = args.ActionArguments["appId"];

                        return appId?.ToString();
                    }
                },
                {
                    "Config.Rollback", (args, permission, configService) =>
                    {
                        var timelineId = args.ActionArguments["publishTimelineId"] as string;
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var detail = configService.GetPublishDetailByPublishTimelineIdAsync(timelineId, env)
                            .GetAwaiter().GetResult();
                        return detail.FirstOrDefault()?.AppId;
                    }
                },
                {
                    "Config.Rollback_API", (args, permission, configService) =>
                    {
                        var timelineId = args.ActionArguments["historyId"] as string;
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var detail = configService.GetPublishDetailByPublishTimelineIdAsync(timelineId, env)
                            .GetAwaiter().GetResult();
                        return detail.FirstOrDefault()?.AppId;
                    }
                },
                {
                    "App.Add", (args, permission, configService) => ""
                },
                {
                    "App.Edit", (args, permission, configService) =>
                    {
                        var app = args.ActionArguments["model"] as IAppModel;
                        return app.Id;
                    }
                },
                {
                    "App.Delete", (args, permission, configService) =>
                    {
                        var id = args.ActionArguments["id"] as string;
                        return id;
                    }
                },
                {
                    "App.DisableOrEnable", (args, permission, configService) =>
                    {
                        var id = args.ActionArguments["id"] as string;
                        return id;
                    }
                },
                {
                    "App.Auth", (args, permission, configService) =>
                    {
                        var model = args.ActionArguments["model"] as IAppIdModel;
                        return model?.AppId;
                    }
                },
                {
                    "Node.Add", (args, permission, configService) =>
                    {
                        var id = args.ActionArguments["id"] as string;
                        return id;
                    }
                },
                {
                    "Node.Delete", (args, permission, configService) =>
                    {
                        var model = args.ActionArguments["model"] as IAppIdModel;
                        return model?.AppId;
                    }
                }
            }.ToFrozenDictionary();

    private readonly string _actionName;
    private readonly IConfigService _configService;
    private readonly string _functionKey;

    private readonly IPermissionService _permissionService;

    public PermissionCheckAttribute(IPermissionService permissionService, IConfigService configService,
        string actionName, string functionKey)
    {
        _permissionService = permissionService;
        _configService = configService;

        _actionName = actionName;
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

        //judge global
        var matchKey = _functionKey;
        if (userFunctions.Contains(_functionKey))
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var appId = "";
        var isAppAction = _actionName.StartsWith("App.");
        if (!isAppAction && GetAppIdParamFuncs.TryGetValue(_actionName, out var func))
            appId = func(context, _permissionService, _configService);

        if (!isAppAction && !string.IsNullOrEmpty(appId))
        {
            matchKey = string.Format(AppMatchPatten, appId, _functionKey);
            if (userFunctions.Contains(matchKey))
            {
                await base.OnActionExecutionAsync(context, next);
                return;
            }
        }

        //no permission
        context.HttpContext.Response.StatusCode = 403;
        context.Result = new ContentResult();
        await base.OnActionExecutionAsync(context, next);
    }
}
