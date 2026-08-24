using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Apisite.Application;

public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor, IUserService userService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public string UserName
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var userName = context.GetUserNameFromClaim();
            return string.IsNullOrEmpty(userName)
                ? context.Request.GetUserNamePasswordFromBasicAuthorization().Item1
                : userName;
        }
    }

    public async Task<string> GetUserIdAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        var userId = context.GetUserIdFromClaim();
        if (!string.IsNullOrEmpty(userId)) return userId;

        var userName = context.Request.GetUserNamePasswordFromBasicAuthorization().Item1;
        if (string.IsNullOrEmpty(userName)) return null;

        var user = (await _userService.GetUsersByNameAsync(userName))
            .FirstOrDefault(x => x.Status == UserStatus.Normal);
        return user?.Id;
    }
}
