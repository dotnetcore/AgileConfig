using System.Text;

namespace AgileConfig.Server.OIDC.TokenEndpointAuthMethods;

internal class BasicTokenEndpointAuthMethod : ITokenEndpointAuthMethod
{
    public (HttpContent HttpContent, string BasicAuthorizationString) GetAuthHttpContent(string code,
        OidcSetting oidcSetting)
    {
        var kvs = new List<KeyValuePair<string, string>>
        {
            new("code", code),
            new("grant_type", "authorization_code"),
            new("redirect_uri", oidcSetting.RedirectUri)
        };
        var httpContent = new FormUrlEncodedContent(kvs);

        var txt = $"{oidcSetting.ClientId}:{oidcSetting.ClientSecret}";
        var authorizationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(txt));

        return (httpContent, authorizationString);
    }
}