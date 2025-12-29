using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Service;

public class AppBasicAuthService : IAppBasicAuthService
{
    private readonly IAppService _appService;

    public AppBasicAuthService(IAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    ///     Parse the appId and secret from the HTTP request.
    /// </summary>
    /// <param name="httpRequest">Incoming HTTP request containing the Authorization header.</param>
    /// <returns>Tuple of Application ID and secret extracted from the header.</returns>
    public (string, string) GetAppIdSecret(HttpRequest httpRequest)
    {
        return Encrypt.UnboxBasicAuth(httpRequest);
    }

    public async Task<bool> ValidAsync(HttpRequest httpRequest)
    {
        var appIdSecret = GetAppIdSecret(httpRequest);
        var appId = appIdSecret.Item1;
        var sec = appIdSecret.Item2;
        if (string.IsNullOrEmpty(appIdSecret.Item1)) return false;

        var app = await _appService.GetAsync(appId);
        if (app == null) return false;
        if (!app.Enabled) return false;

        var txt = $"{app.Id}:{app.Secret}";

        return txt == $"{appId}:{sec}";
    }

    public (string, string) GetUserNamePassword(HttpRequest httpRequest)
    {
        throw new NotImplementedException();
    }
}