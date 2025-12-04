namespace AgileConfig.Server.OIDC;

public interface IOidcClient
{
    OidcSetting OidcSetting { get; }
    string GetAuthorizeUrl();
    (string Id, string UserName) UnboxIdToken(string idToken);
    Task<(string IdToken, string accessToken)> Validate(string code);
}