namespace AgileConfig.Server.OIDC.TokenEndpointAuthMethods
{
    internal static class TokenEndpointAuthMethodFactory
    {
        public static ITokenEndpointAuthMethod Create(string methodName)
        {
            if (methodName == "client_secret_basic")
            {
                return new BasicTokenEndpointAuthMethod();
            }
            else if (methodName == "client_secret_post")
            {
                return new PostTokenEndpointAuthMethod();
            }
            else if (methodName == "none")
            {
                return new NoneTokenEndpointAuthMethod();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
