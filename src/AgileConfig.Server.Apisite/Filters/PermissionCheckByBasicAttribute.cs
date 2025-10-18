using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Filters
{
    /// <summary>
    /// Permission check that reads user information from the basic authentication header.
    /// </summary>
    public class PermissionCheckByBasicAttribute : PermissionCheckAttribute
    {
        protected IAdmBasicAuthService _basicAuthService;
        protected IUserService _userService;

        public PermissionCheckByBasicAttribute(
            IPermissionService permissionService, 
            IConfigService configService,
            IAdmBasicAuthService basicAuthService, 
            IUserService userService,
            string actionName, 
            string functionKey) : base(permissionService, configService, actionName, functionKey)
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
