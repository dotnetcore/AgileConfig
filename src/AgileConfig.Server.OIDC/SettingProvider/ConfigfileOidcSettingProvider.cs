using AgileConfig.Server.Common;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.OIDC.SettingProvider;

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
        var tokenEndpointAuthMethod = Global.Config["SSO:OIDC:tokenEndpointAuthMethod"];
        var loginButtonText = Global.Config["SSO:loginButtonText"];

        _oidcSetting = new OidcSetting(
            clientId,
            clientSecret, redirectUri,
            tokenEndpoint, authorizationEndpoint,
            userIdClaim, userNameClaim,
            scope, tokenEndpointAuthMethod);

        logger.LogInformation($"OIDC Setting " +
                              $"clientId:{clientId} " +
                              $"redirectUri:{redirectUri} " +
                              $"tokenEndpoint:{tokenEndpoint} " +
                              $"authorizationEndpoint:{authorizationEndpoint} " +
                              $"userIdClaim:{userIdClaim} " +
                              $"userNameClaim:{userNameClaim} " +
                              $"scope:{scope} " +
                              $"loginButtonText:{loginButtonText} " +
                              $"tokenEndpointAuthMethod:{tokenEndpointAuthMethod} "
        );
    }

    public OidcSetting GetSetting()
    {
        return _oidcSetting;
    }
}