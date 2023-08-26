namespace AgileConfig.Server.OIDC.TokenEndpointAuthMethods
{
    internal class PostTokenEndpointAuthMethod : ITokenEndpointAuthMethod
    {
        public (HttpContent HttpContent, string BasicAuthorizationString) GetAuthHttpContent(string code, OidcSetting oidcSetting)
        {
            var kvs = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", oidcSetting.RedirectUri),
                new KeyValuePair<string, string>("client_id", oidcSetting.ClientId),
                new KeyValuePair<string, string>("client_secret", oidcSetting.ClientSecret),
            };
            var httpContent = new FormUrlEncodedContent(kvs);

            return (httpContent, "");
        }
    }
}
