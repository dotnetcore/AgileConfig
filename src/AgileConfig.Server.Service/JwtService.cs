using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using Microsoft.IdentityModel.Tokens;

namespace AgileConfig.Server.Service;

/// <summary>
/// jwt 相关业务
/// </summary>
public class JwtService : IJwtService
{
    private readonly ISettingService _settingService;
    // static JwtService()
    // {
    //     // 则尝试生成一个key到数据库
    //     using var settingService = new SettingService(new FreeSqlContext(FreeSQL.Instance));
    //     settingService.TryInitJwtSecret();
    // }

    public JwtService(ISettingService settingService)
    {
        _settingService = settingService;
    }
    
    public  string Issuer => Global.Config["JwtSetting:Issuer"];
    public  string Audience => Global.Config["JwtSetting:Audience"];
    public  int ExpireSeconds => int.Parse(Global.Config["JwtSetting:ExpireSeconds"]);

    private  string _secretKey;
    public  string GetSecurityKey()
    {
        if (!string.IsNullOrEmpty(_secretKey))
        {
            return _secretKey;
        }
        
        _secretKey = Global.Config["JwtSetting:SecurityKey"];
        if (!string.IsNullOrEmpty(_secretKey))
        {
            return _secretKey;
        }

        //using var settingService = new SettingService(new FreeSqlContext(FreeSQL.Instance));
        _secretKey = _settingService.GetJwtTokenSecret();

        if (string.IsNullOrEmpty(_secretKey))
        {
            throw new ArgumentNullException($"No JwtSetting SecurityKey");
        }
        
        return _secretKey;
    }

    public  string GetToken(string userId, string userName, bool isAdmin)
    {
        //创建用户身份标识，可按需要添加更多信息
        var claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", userId, ClaimValueTypes.String), // 用户id
            new Claim("username", userName, ClaimValueTypes.String), // 用户名
            new Claim("admin", isAdmin.ToString() ,ClaimValueTypes.Boolean) // 是否是管理员
        };
        var key = Encoding.UTF8.GetBytes(GetSecurityKey());
        //创建令牌
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddSeconds(ExpireSeconds)
        );

        string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return jwtToken;
    }
}