using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.IService;
using Microsoft.IdentityModel.Tokens;

namespace AgileConfig.Server.Service;

/// <summary>
///     JWT-related service operations.
/// </summary>
public class JwtService(ISysInitRepository sysInitRepository) : IJwtService
{
    private string _secretKey;
    // static JwtService()
    // {
    //     // Ensure a secret key exists in the database when initializing.
    //     using var settingService = new SettingService(new FreeSqlContext(FreeSQL.Instance));
    //     settingService.TryInitJwtSecret();
    // }

    public string Issuer => Global.Config["JwtSetting:Issuer"];
    public string Audience => Global.Config["JwtSetting:Audience"];
    public int ExpireSeconds => int.Parse(Global.Config["JwtSetting:ExpireSeconds"]);

    public string GetSecurityKey()
    {
        if (!string.IsNullOrEmpty(_secretKey)) return _secretKey;

        _secretKey = Global.Config["JwtSetting:SecurityKey"];
        if (!string.IsNullOrEmpty(_secretKey)) return _secretKey;

        //using var settingService = new SettingService(new FreeSqlContext(FreeSQL.Instance));
        _secretKey = sysInitRepository.GetJwtTokenSecret();

        if (string.IsNullOrEmpty(_secretKey)) throw new ArgumentNullException("No JwtSetting SecurityKey");

        return _secretKey;
    }

    public string GetToken(string userId, string userName, bool isAdmin)
    {
        // Create user claims; add more information as needed.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", userId, ClaimValueTypes.String), // User identifier.
            new Claim("username", userName, ClaimValueTypes.String), // User name.
            new Claim("admin", isAdmin.ToString(), ClaimValueTypes.Boolean) // Whether the user is an administrator.
        };
        var key = Encoding.UTF8.GetBytes(GetSecurityKey());
        // Create the JWT token.
        var token = new JwtSecurityToken(
            Issuer,
            Audience,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddSeconds(ExpireSeconds)
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return jwtToken;
    }
}