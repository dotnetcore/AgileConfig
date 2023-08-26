namespace AgileConfig.Server.OIDC.TokenEndpointAuthMethods
{
    internal interface ITokenEndpointAuthMethod
    {
        (HttpContent HttpContent, string BasicAuthorizationString) GetAuthHttpContent(string code, OidcSetting oidcSetting);
    }
}
