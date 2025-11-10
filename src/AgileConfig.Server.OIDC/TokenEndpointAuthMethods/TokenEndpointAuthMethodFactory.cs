namespace AgileConfig.Server.OIDC.TokenEndpointAuthMethods;

internal static class TokenEndpointAuthMethodFactory
{
    public static ITokenEndpointAuthMethod Create(string methodName)
    {
        if (methodName == "client_secret_basic") return new BasicTokenEndpointAuthMethod();

        if (methodName == "client_secret_post") return new PostTokenEndpointAuthMethod();

        if (methodName == "none") return new NoneTokenEndpointAuthMethod();

        throw new NotImplementedException();
    }
}