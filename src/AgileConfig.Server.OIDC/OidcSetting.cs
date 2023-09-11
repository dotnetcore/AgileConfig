namespace AgileConfig.Server.OIDC
{
    public class OidcSetting
    {
        public OidcSetting(
            string clientId, 
            string clientSecret, 
            string redirectUri, 
            string tokenEndpoint, 
            string authorizationEndpoint, 
            string userIdClaim, 
            string userNameClaim, 
            string scope,
            string tokenEndpointAuthMethod
            )
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
            TokenEndpoint = tokenEndpoint;
            AuthorizationEndpoint = authorizationEndpoint;
            UserIdClaim = userIdClaim;
            UserNameClaim = userNameClaim;
            Scope = scope;
            TokenEndpointAuthMethod = tokenEndpointAuthMethod;
        }

        public string ClientId { get; }
        public string ClientSecret { get; }
        public string RedirectUri { get; }
        public string TokenEndpoint { get; }
        public string AuthorizationEndpoint { get; }
        public string UserIdClaim { get; }
        public string UserNameClaim { get; }
        public string Scope { get; }
        public string TokenEndpointAuthMethod { get; set; }
    }
}
