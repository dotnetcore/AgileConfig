namespace AgileConfig.Server.OIDC.TokenEndpointAuthMethods;

internal class PostTokenEndpointAuthMethod : ITokenEndpointAuthMethod
{
    public (HttpContent HttpContent, string BasicAuthorizationString) GetAuthHttpContent(string code,
        OidcSetting oidcSetting)
    {
        var kvs = new List<KeyValuePair<string, string>>
        {
            new("code", code),
            new("grant_type", "authorization_code"),
            new("redirect_uri", oidcSetting.RedirectUri),
            new("client_id", oidcSetting.ClientId),
            new("client_secret", oidcSetting.ClientSecret)
        };
        var httpContent = new FormUrlEncodedContent(kvs);

        return (httpContent, "");
    }
}