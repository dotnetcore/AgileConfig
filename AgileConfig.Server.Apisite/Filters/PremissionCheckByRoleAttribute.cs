using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Filters
{
    public class PremissionCheckByRoleAttribute : ActionFilterAttribute
    {
        private Role[] _roles;
        private IPremissionService _permissionService;
        public PremissionCheckByRoleAttribute(IPremissionService permissionService, Role[] roles)
        {
            _permissionService = permissionService;
            _roles = roles;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.GetUserIdFromClaim();
            var userFunctions = await _permissionService.GetUserPermission(userId);

            //no permission
            context.HttpContext.Response.StatusCode = 403;
            context.Result = new ContentResult();
            await base.OnActionExecutionAsync(context, next);
        }


    }
}
