namespace AgileConfig.Server.OIDC.TokenEndpointAuthMethods;

internal class NoneTokenEndpointAuthMethod : ITokenEndpointAuthMethod
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

        return (httpContent, "");
    }
}