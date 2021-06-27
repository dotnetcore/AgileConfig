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
        static Dictionary<string, Func<ActionExecutingContext, IPermissionService, IConfigService, string>> _getAppIdParamFuncs = new Dictionary<string, Func<ActionExecutingContext, IPermissionService, IConfigService, string>>
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
            }
        };

        private const string _globalMatchPatten = "GLOBAL_{0}";
        private const string _appMatchPatten = "APP_{0}_{1}";

        private IPermissionService _premissionService;
        private IConfigService _configService;
        private string _actionName;
        private string _functionKey;
        public PremissionCheckAttribute(IPermissionService premissionService, IConfigService configService, string actionName, string functionKey)
        {
            _premissionService = premissionService;
            _configService = configService;
            _actionName = actionName;
            _functionKey = functionKey;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.GetUserIdFromClaim();
            var userFunctions = await _premissionService.GetUserPermission(userId);

            //judge global
            var matchKey = string.Format(_globalMatchPatten, _functionKey);
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
                matchKey = string.Format(_appMatchPatten, appId, _functionKey);
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
