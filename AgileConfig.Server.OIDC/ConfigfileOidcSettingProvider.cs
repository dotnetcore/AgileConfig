using AgileConfig.Server.Common;

namespace AgileConfig.Server.OIDC
{
    public class ConfigfileOidcSettingProvider : IOidcSettingProvider
    {
        public OidcSetting GetSetting()
        {
            return new OidcSetting()
            {
                ClientId = Global.Config["OIDC:clientId"],
                ClientSecret = Global.Config["OIDC:clientSecret"],
                TokenEndpoint = Global.Config["OIDC:tokenEndpoint"],
                AuthorizationEndpoint = Global.Config["OIDC:authorizationEndpoint"],
                RedirectUri = Global.Config["OIDC:redirectUri"],
            };
        }
    }
}
