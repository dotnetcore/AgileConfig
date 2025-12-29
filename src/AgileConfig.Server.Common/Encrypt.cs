using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Common;

public static class Encrypt
{
    private static readonly ThreadLocal<MD5> Md5Instance = new(MD5.Create);

    public static string Md5(string txt)
    {
        var inputBytes = Encoding.ASCII.GetBytes(txt);
        var hashBytes = Md5Instance.Value.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes);
    }

    public static (string, string) UnboxBasicAuth(HttpRequest httpRequest)
    {
        var authorization = httpRequest.Headers["Authorization"];
        if (string.IsNullOrEmpty(authorization)) return ("", "");
        var authStr = authorization.First();
        // Remove the "Basic " prefix.
        if (!authStr.StartsWith("Basic "))
        {
            return ("", "");
            ;
        }

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

        var appId = "";
        var sec = "";


        var baseAuthArr = base64Str.Split(':');

        if (baseAuthArr.Length > 0) appId = baseAuthArr[0];
        if (baseAuthArr.Length > 1) sec = baseAuthArr[1];

        return (appId, sec);
    }
}