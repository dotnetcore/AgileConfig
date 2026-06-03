using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Apisite.Filters;

/// <summary>
///     Permission check that reads user information from the basic authentication header.
/// </summary>
public class PermissionCheckByBasicAttribute : PermissionCheckAttribute
{
    public PermissionCheckByBasicAttribute(
        IPermissionService permissionService,
        IConfigService configService,
        string functionKey) : base(permissionService, configService, functionKey)
    {
    }

    protected override async Task<string> GetUserId(ActionExecutingContext context)
    {
        var services = context.HttpContext.RequestServices;
        var basicAuthService = services.GetRequiredService<IAdmBasicAuthService>();
        var userService = services.GetRequiredService<IUserService>();
        
        var userName = basicAuthService.GetUserNamePassword(context.HttpContext.Request).Item1;
        var user = (await userService.GetUsersByNameAsync(userName)).FirstOrDefault(x =>
            x.Status == UserStatus.Normal);

        return user?.Id;
    }
}