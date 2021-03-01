using AgileConfig.Server.Common;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite
{
    public class JwtSetting
    {
        static JwtSetting()
        {
            Instance = new JwtSetting();
            Instance.Audience = Global.Config["JwtSetting:Audience"];
            Instance.SecurityKey = Global.Config["JwtSetting:SecurityKey"];
            Instance.Issuer = Global.Config["JwtSetting:Issuer"];
            Instance.ExpireSeconds = int.Parse(Global.Config["JwtSetting:ExpireSeconds"]);
        }

        public string SecurityKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int ExpireSeconds { get; set; }

        public static JwtSetting Instance
        {
            get;
        }
    }
    public class JWT
    {
        public static string GetToken()
        {
            //创建用户身份标识，可按需要添加更多信息
            var claims = new Claim[]
            {
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim("id", "admin", ClaimValueTypes.String), // 用户id
    new Claim("name", "admin"), // 用户名
    new Claim("admin", true.ToString() ,ClaimValueTypes.Boolean) // 是否是管理员
            };
            var key = Encoding.UTF8.GetBytes(JwtSetting.Instance.SecurityKey);
            //创建令牌
            var token = new JwtSecurityToken(
              issuer: JwtSetting.Instance.Issuer,
              audience: JwtSetting.Instance.Audience,
              signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
              claims: claims,
              notBefore: DateTime.Now,
              expires: DateTime.Now.AddSeconds(JwtSetting.Instance.ExpireSeconds)
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }
    }
}
