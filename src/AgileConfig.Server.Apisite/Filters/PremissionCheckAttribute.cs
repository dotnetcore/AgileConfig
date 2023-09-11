using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Filters
{
    public class PremissionCheckAttribute : ActionFilterAttribute
    {
        private static string GetEnvFromArgs(IDictionary<string, object> args, IConfigService configService)
        {
            args.TryGetValue("env", out object env);
            var envStr = "";
            if (env == null)
            {
                envStr = configService.IfEnvEmptySetDefaultAsync(null).GetAwaiter().GetResult();
            }
            else
            {
                envStr = env.ToString();
            }

            return envStr;
        }

        /// <summary>
        /// 因为 attribute 不能传递 func 参数，所有从 action 的参数内获取 appId 的操作只能提前内置在一个静态字典内。
        /// </summary>
        protected static Dictionary<string, Func<ActionExecutingContext, IPremissionService, IConfigService, string>> _getAppIdParamFuncs = new Dictionary<string, Func<ActionExecutingContext, IPremissionService, IConfigService, string>>
        {
            {
                "Config.Add",(args, premission, config)=> { var model = args.ActionArguments["model"];  return (model as IAppIdModel)?.AppId; }
            },
             {
                "Config.AddRange",(args, premission, config)=> { var model = args.ActionArguments["model"];  return (model as List<ConfigVM>)?.FirstOrDefault()?.AppId; }
            },
             {
                 "Config.EnvSync",(args, premission, config)=> { var appId = args.ActionArguments["appId"];  return appId?.ToString(); }
             },
            {
                "Config.Edit",(args, premission, config)=> { var model = args.ActionArguments["model"];  return (model as IAppIdModel)?.AppId; }
            },
            {
                "Config.Delete", (args, premission, configService) =>  {
                        var id = args.ActionArguments["id"];
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(id.ToString(),env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            },
               {
                "Config.DeleteSome", (args, premission, configService) =>  {
                        var ids = args.ActionArguments["ids"] as List<string>;
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(ids.FirstOrDefault(),env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            },
            {
                "Config.Offline", (args, premission, configService) =>  {
                        var id = args.ActionArguments["configId"] ;
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(id.ToString(),env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            }
            ,
            {
                "Config.OfflineSome", (args, premission, configService) =>  {
                        var ids = args.ActionArguments["configIds"] as List<string>;
                        var id = ids?.FirstOrDefault();
                        var env = GetEnvFromArgs(args.ActionArguments, configService);
                        var config = configService.GetAsync(ids.FirstOrDefault(),env).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            }
            ,
            {
                "Config.Publish", (args, premission, configService) =>  {
                        var model = args.ActionArguments["model"] as IAppIdModel;

                        return model?.AppId;
                    }
            }
            ,
            {
                "Config.Publish_API", (args, premission, configService) =>  {
                        var appId = args.ActionArguments["appId"];

                        return appId.ToString();
                    }
            }
            ,
             {
                "Config.Rollback", (args, premission, configService) =>  {
                        var timelineId = args.ActionArguments["publishTimelineId"] as string;
                        var env =  GetEnvFromArgs(args.ActionArguments, configService);
                        var detail = configService.GetPublishDetailByPublishTimelineIdAsync(timelineId, env).GetAwaiter().GetResult();
                        return detail.FirstOrDefault()?.AppId;
                    }
            }
             ,
             {
                "Config.Rollback_API", (args, premission, configService) =>  {
                        var timelineId = args.ActionArguments["historyId"] as string;
                        var env =  GetEnvFromArgs(args.ActionArguments, configService);
                        var detail = configService.GetPublishDetailByPublishTimelineIdAsync(timelineId, env).GetAwaiter().GetResult();
                        return detail.FirstOrDefault()?.AppId;
                    }
            }
            ,
             {
                "App.Add", (args, premission, configService) =>  {
                    return  "";
                    }
            },
             {
                "App.Edit", (args, premission, configService) =>  {
                      var app = args.ActionArguments["model"] as IAppModel;
                      return app.Id;
                }
            },
             {
                "App.Delete", (args, premission, configService) =>  {
                    var id = args.ActionArguments["id"] as string;
                    return id;
                }
            },
             {
                "App.DisableOrEanble", (args, premission, configService) =>  {
                    var id = args.ActionArguments["id"] as string;
                    return id;
                }
            },
             {
                "App.Auth", (args, premission, configService) =>  {
                    var model = args.ActionArguments["model"] as IAppIdModel;
                    return model?.AppId;
                }
            },
             {
                "Node.Add", (args, premission, configService) =>  {
                    var id = args.ActionArguments["id"] as string;
                    return id;
                }
            },
             {
                "Node.Delete", (args, premission, configService) =>  {
                    var model = args.ActionArguments["model"] as IAppIdModel;
                    return model?.AppId;
                }
            }
        };

        protected const string GlobalMatchPatten = "GLOBAL_{0}";
        protected const string AppMatchPatten = "APP_{0}_{1}";

        private IPremissionService _premissionService;
        private IConfigService _configService;

        private string _actionName;
        private string _functionKey;
        public PremissionCheckAttribute(IPremissionService premissionService, IConfigService configService, string actionName, string functionKey)
        {
            _premissionService = premissionService;
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

            var userFunctions = await _premissionService.GetUserPermission(userId);

            //judge global
            var matchKey = string.Format(GlobalMatchPatten, _functionKey);
            if (userFunctions.Contains(matchKey))
            {
                await base.OnActionExecutionAsync(context, next);
                return;
            }
            var appId = "";
            if (_getAppIdParamFuncs.ContainsKey(_actionName))
            {
                var func = _getAppIdParamFuncs[_actionName];
                appId = func(context, _premissionService, _configService);
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
