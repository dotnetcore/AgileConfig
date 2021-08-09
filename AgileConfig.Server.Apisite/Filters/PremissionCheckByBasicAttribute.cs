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
    /// <summary>
    /// 权限判断，用户信息从basic认证的头部取
    /// </summary>
    public class PremissionCheckByBasicAttribute : PremissionCheckAttribute
    {
        protected IAdmBasicAuthService _basicAuthService;
        protected IUserService _userService;

        public PremissionCheckByBasicAttribute(
            IPremissionService premissionService, 
            IConfigService configService,
            IAdmBasicAuthService basicAuthService, 
            IUserService userService,
            string actionName, 
            string functionKey) : base(premissionService, configService, actionName, functionKey)
        {
            _userService = userService;
            _basicAuthService = basicAuthService;
        }

        protected override async Task<string> GetUserId(ActionExecutingContext context)
        {
            var userName = _basicAuthService.GetUserNamePassword(context.HttpContext.Request).Item1;
            var user = (await _userService.GetUsersByNameAsync(userName)).FirstOrDefault(x => x.Status == Data.Entity.UserStatus.Normal);

            return user?.Id;
        }
    }
}
