using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Filters
{
    public class PremissionCheckAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 因为 attribute 不能传递 func 参数，所有从 action 的参数内获取 appId 的操作只能提前内置在一个静态字典内。
        /// </summary>
        protected static Dictionary<string, Func<ActionExecutingContext, IPremissionService, IConfigService, string>> _getAppIdParamFuncs = new Dictionary<string, Func<ActionExecutingContext, IPremissionService, IConfigService, string>>
        {
            {
                "Config.Add",(args, premission, config)=> { var model = args.ActionArguments["model"];  return (model as ConfigVM)?.AppId; }
            },
             {
                "Config.AddRange",(args, premission, config)=> { var model = args.ActionArguments["model"];  return (model as List<ConfigVM>)?.FirstOrDefault()?.AppId; }
            },
            {
                "Config.Edit",(args, premission, config)=> { var model = args.ActionArguments["model"];  return (model as ConfigVM)?.AppId; }
            },
            {
                "Config.Delete", (args, premission, configService) =>  {
                        var id = args.ActionArguments["id"];
                        var config = configService.GetAsync(id.ToString()).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            },
            {
                "Config.Offline", (args, premission, configService) =>  {
                        var id = args.ActionArguments["configId"] ;
                        var config = configService.GetAsync(id.ToString()).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            }
            ,
            {
                "Config.OfflineSome", (args, premission, configService) =>  {
                        var ids = args.ActionArguments["configIds"] as List<string>;
                        var id = ids?.FirstOrDefault();
                        var config = configService.GetAsync(id.ToString()).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            }
            ,
            {
                "Config.PublishAsync", (args, premission, configService) =>  {
                        var id = args.ActionArguments["configId"] ;
                        var config = configService.GetAsync(id.ToString()).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            }
            ,
            {
                "Config.PublishSome", (args, premission, configService) =>  {
                        var ids = args.ActionArguments["configIds"] as List<string>;
                        var id = ids?.FirstOrDefault();
                        var config = configService.GetAsync(id.ToString()).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            },
             {
                "Config.Rollback", (args, premission, configService) =>  {
                        var id = args.ActionArguments["configId"] as string;
                        var config = configService.GetAsync(id.ToString()).GetAwaiter().GetResult();

                        return config.AppId;
                    }
            },
             {
                "App.Add", (args, premission, configService) =>  {
                    return  "";
                    }
            },
             {
                "App.Edit", (args, premission, configService) =>  {
                      var app = args.ActionArguments["model"] as AppVM;
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
                    var model = args.ActionArguments["model"] as AppAuthVM;
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
                    var model = args.ActionArguments["model"] as AppAuthVM;
                    return model?.AppId;
                }
            }
        };

        protected const string GlobalMatchPatten = "GLOBAL_{0}";
        protected const string AppMatchPatten = "APP_{0}_{1}";

        protected IPremissionService _premissionService;
        protected IConfigService _configService;
        protected string _actionName;
        protected string _functionKey;
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
