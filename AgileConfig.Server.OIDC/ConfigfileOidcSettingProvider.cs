using AgileConfig.Server.Common;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.OIDC
{
    public class ConfigfileOidcSettingProvider : IOidcSettingProvider
    {
        private readonly OidcSetting _oidcSetting;

        public ConfigfileOidcSettingProvider(ILogger<ConfigfileOidcSettingProvider> logger)
        {
            var clientId = Global.Config["SSO:OIDC:clientId"];
            var clientSecret = Global.Config["SSO:OIDC:clientSecret"];
            var tokenEndpoint = Global.Config["SSO:OIDC:tokenEndpoint"];
            var authorizationEndpoint = Global.Config["SSO:OIDC:authorizationEndpoint"];
            var redirectUri = Global.Config["SSO:OIDC:redirectUri"];
            var userIdClaim = Global.Config["SSO:OIDC:userIdClaim"];
            var userNameClaim = Global.Config["SSO:OIDC:userNameClaim"];
            var scope = Global.Config["SSO:OIDC:scope"];
            var loginButtonText = Global.Config["SSO:loginButtonText"];

            _oidcSetting = new OidcSetting(
                clientId, 
                clientSecret, redirectUri, 
                tokenEndpoint, authorizationEndpoint, 
                userIdClaim, userNameClaim, 
                scope);

            logger.LogInformation($"OIDC Setting " +
                $"clientId:{clientId} " +
                $"redirectUri:{redirectUri} " +
                $"tokenEndpoint:{tokenEndpoint} " +
                $"authorizationEndpoint:{authorizationEndpoint} " +
                $"userIdClaim:{userIdClaim} " +
                $"userNameClaim:{userNameClaim} " +
                $"scope:{scope} " +
                $"loginButtonText:{loginButtonText} "
                );
        }

        public OidcSetting GetSetting()
        {
            return _oidcSetting;
        }
    }
}
