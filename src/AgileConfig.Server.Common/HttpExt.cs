using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Common;

public static class HttpExt
{
    /// <summary>
    ///     Parse the username and password from the Authorization header of a request.
    /// </summary>
    /// <param name="httpRequest">HTTP request containing the Authorization header.</param>
    /// <returns>Tuple of username and password extracted from the header.</returns>
    public static (string, string) GetUserNamePasswordFromBasicAuthorization(this HttpRequest httpRequest)
    {
        var authorization = httpRequest.Headers["Authorization"];
        if (string.IsNullOrEmpty(authorization)) return ("", "");
        var authStr = authorization.First();
        // Strip the "Basic " prefix.
        if (!authStr.StartsWith("Basic ")) return ("", "");
        authStr = authStr.Substring(6, authStr.Length - 6);
        byte[] base64Decode = null;
        try
        {
            base64Decode = Convert.FromBase64String(authStr);
        }
        catch
        {
            return ("", "");
        }

        var base64Str = Encoding.UTF8.GetString(base64Decode);

        if (string.IsNullOrEmpty(base64Str)) return ("", "");

        var userName = "";
        var password = "";
        var baseAuthArr = base64Str.Split(':');

        if (baseAuthArr.Length > 0) userName = baseAuthArr[0];
        if (baseAuthArr.Length > 1) password = baseAuthArr[1];

        return (userName, password);
    }


    public static string GetUserNameFromClaim(this HttpContext httpContext)
    {
        var name = httpContext.User?.FindFirst("username")?.Value;

        return name;
    }

    public static string GetUserIdFromClaim(this HttpContext httpContext)
    {
        return httpContext.User?.FindFirst("id")?.Value;
    }
}