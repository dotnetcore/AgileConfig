using System.Text;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AgileConfig.Server.Apisite;

public class ConfigureJwtBearerOptions(
    IJwtService jwtService) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtService.Issuer,
            ValidAudience = jwtService.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtService.GetSecurityKey()))
        };
    }

    public void Configure(string name, JwtBearerOptions options)
    {
        Configure(options);
    }
}