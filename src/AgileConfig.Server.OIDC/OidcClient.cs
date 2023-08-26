using AgileConfig.Server.OIDC.SettingProvider;
using AgileConfig.Server.OIDC.TokenEndpointAuthMethods;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace AgileConfig.Server.OIDC
{
    public class OidcClient : IOidcClient
    {
        private readonly IOidcSettingProvider _oidcSettingProvider;
        private readonly OidcSetting _oidcSetting;

        public OidcSetting OidcSetting => _oidcSetting;

        public OidcClient(IOidcSettingProvider oidcSettingProvider)
        {
            _oidcSettingProvider = oidcSettingProvider;
            _oidcSetting = _oidcSettingProvider.GetSetting();
        }

        public string GetAuthorizeUrl()
        {
            var url = $"{_oidcSetting.AuthorizationEndpoint}?" +
                $"client_id={_oidcSetting.ClientId}" +
                $"&response_type=code" +
                $"&scope={_oidcSetting.Scope}" +
                $"&redirect_uri={_oidcSetting.RedirectUri}" +
                $"&state={Guid.NewGuid()}";
            return url;
        }

        public async Task<(string IdToken, string accessToken)> Validate(string code)
        {
            var authMethod = TokenEndpointAuthMethodFactory.Create(_oidcSetting.TokenEndpointAuthMethod);
            var httpContent = authMethod.GetAuthHttpContent(code, _oidcSetting);

            using var httpclient = new HttpClient();
            if (!string.IsNullOrEmpty(httpContent.BasicAuthorizationString))
            {
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", httpContent.BasicAuthorizationString);
            }
            var response = await httpclient.PostAsync(_oidcSetting.TokenEndpoint, httpContent.HttpContent);
            response.EnsureSuccessStatusCode();

            var bodyJson = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(bodyJson))
            {
                throw new Exception("Can not validate the code. Token endpoint return empty response.");
            }

            var responseObject = JsonConvert.DeserializeObject<TokenEndpointResponseModel>(bodyJson);
            string id_token = responseObject.id_token;

            if (string.IsNullOrWhiteSpace(id_token))
            {
                throw new Exception("Can not validate the code. Id token missing.");
            }

            return (id_token, "");
        }

        public (string Id, string UserName) UnboxIdToken(string idToken)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            var id = jwtToken.Claims.Where(x => x.Type == _oidcSetting.UserIdClaim).FirstOrDefault()?.Value;
            var userName = jwtToken.Claims.Where(x => x.Type == _oidcSetting.UserNameClaim).FirstOrDefault()?.Value;

            return (id, userName);
        }
    }

    internal class TokenEndpointResponseModel
    {
        public string id_token { get; set; }

        public string access_token { get; set; }
    }
}