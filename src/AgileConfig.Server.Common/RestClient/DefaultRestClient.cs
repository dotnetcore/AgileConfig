using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common.RestClient
{
    public class DefaultRestClient : IRestClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DefaultRestClient(
            IHttpClientFactory httpClientFactory
            )
        {
            this._httpClientFactory = httpClientFactory;
        }

        private HttpClient GetDefaultClient()
        {
            return _httpClientFactory.CreateClient(Global.DefaultHttpClientName);
        }

        public async Task<T> GetAsync<T>(string url, Dictionary<string, string> headers = null)
        {
            using var resp = await GetAsync(url, headers);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T> PostAsync<T>(string url, object data, Dictionary<string, string> headers = null)
        {
            using var resp = await PostAsync(url, data, headers);

            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object data, Dictionary<string, string> headers = null)
        {
            var httpclient = GetDefaultClient();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpclient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var jsondata = "";
            if (data != null)
            {
                jsondata = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            }
            var stringContent = new StringContent(jsondata,
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

            var resp = await httpclient.PostAsync(url, stringContent);

            return resp;
        }

        public async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers = null)
        {
            var httpclient = GetDefaultClient();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpclient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            var resp = await httpclient.GetAsync(url);

            return resp;
        }
    }
}
