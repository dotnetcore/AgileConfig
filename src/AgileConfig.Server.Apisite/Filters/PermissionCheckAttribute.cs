using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Filters
{
    public class PermissionCheckAttribute : ActionFilterAttribute
    {
        private static string GetEnvFromArgs(IDictionary<string, object> args, IConfigService configService)
        {
            args.TryGetValue("env", out object envArg);
            var env = "";
            if (envArg == null)
            {
                ISettingService.IfEnvEmptySetDefault(ref env);
            }
            else
            {
                env = envArg.ToString();
            }

            return env;
        }

        /// <summary>
        /// 因为 attribute 不能传递 func 参数，所有从 action 的参数内获取 appId 的操作只能提前内置在一个静态字典内。
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

        protected const string GlobalMatchPatten = "GLOBAL_{0}";
        protected const string AppMatchPatten = "APP_{0}_{1}";

        private readonly IPermissionService _permissionService;
        private readonly IConfigService _configService;

        private readonly string _actionName;
        private readonly string _functionKey;

        public PermissionCheckAttribute(IPermissionService permissionService, IConfigService configService,
            string actionName, string functionKey)
        {
            _permissionService = permissionService;
            _configService = configService;

            _actionName = actionName;
            _functionKey = functionKey;
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
            var matchKey = string.Format(GlobalMatchPatten, _functionKey);
            if (userFunctions.Contains(matchKey))
            {
                await base.OnActionExecutionAsync(context, next);
                return;
            }

            var appId = "";
            if (GetAppIdParamFuncs.TryGetValue(_actionName, out var func))
            {
                appId = func(context, _permissionService, _configService);
            }

            if (!string.IsNullOrEmpty(appId))
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
}