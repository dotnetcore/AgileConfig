using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using AgileConfig.Server.Common;
using AgileConfig.Server.OIDC.SettingProvider;
using AgileConfig.Server.OIDC.TokenEndpointAuthMethods;
using Newtonsoft.Json;

namespace AgileConfig.Server.OIDC;

public class OidcClient : IOidcClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOidcSettingProvider _oidcSettingProvider;

    public OidcClient(IOidcSettingProvider oidcSettingProvider, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _oidcSettingProvider = oidcSettingProvider;
        OidcSetting = _oidcSettingProvider.GetSetting();
    }

    public OidcSetting OidcSetting { get; }

    public string GetAuthorizeUrl()
    {
        var url = $"{OidcSetting.AuthorizationEndpoint}?" +
                  $"client_id={OidcSetting.ClientId}" +
                  $"&response_type=code" +
                  $"&scope={OidcSetting.Scope}" +
                  $"&redirect_uri={OidcSetting.RedirectUri}" +
                  $"&state={Guid.NewGuid()}";
        return url;
    }

    public async Task<(string IdToken, string accessToken)> Validate(string code)
    {
        var authMethod = TokenEndpointAuthMethodFactory.Create(OidcSetting.TokenEndpointAuthMethod);
        var httpContent = authMethod.GetAuthHttpContent(code, OidcSetting);


        var httpclient = _httpClientFactory.CreateClient(Global.DefaultHttpClientName);
        if (!string.IsNullOrEmpty(httpContent.BasicAuthorizationString))
            httpclient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", httpContent.BasicAuthorizationString);
        var response = await httpclient.PostAsync(OidcSetting.TokenEndpoint, httpContent.HttpContent);
        response.EnsureSuccessStatusCode();

        var bodyJson = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(bodyJson))
            throw new Exception("Can not validate the code. Token endpoint return empty response.");

        var responseObject = JsonConvert.DeserializeObject<TokenEndpointResponseModel>(bodyJson);
        var id_token = responseObject.id_token;

        if (string.IsNullOrWhiteSpace(id_token)) throw new Exception("Can not validate the code. Id token missing.");

        return (id_token, "");
    }

    public (string Id, string UserName) UnboxIdToken(string idToken)
    {
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
        var id = jwtToken.Claims.Where(x => x.Type == OidcSetting.UserIdClaim).FirstOrDefault()?.Value;
        var userName = jwtToken.Claims.Where(x => x.Type == OidcSetting.UserNameClaim).FirstOrDefault()?.Value;

        return (id, userName);
    }
}

internal class TokenEndpointResponseModel
{
    public string id_token { get; set; }

    public string access_token { get; set; }
}