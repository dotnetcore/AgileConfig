using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AgileConfig.Server.Common.RestClient;

public class DefaultRestClient : IRestClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DefaultRestClient(
        IHttpClientFactory httpClientFactory
    )
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<T> GetAsync<T>(string url, Dictionary<string, string> headers = null)
    {
        using var resp = await GetAsync(url, headers);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task<T> PostAsync<T>(string url, object data, Dictionary<string, string> headers = null)
    {
        using var resp = await PostAsync(url, data, headers);

        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    public Task<HttpResponseMessage> PostAsync(string url, object data, Dictionary<string, string> headers = null)
    {
        var httpclient = GetDefaultClient();
        if (headers != null)
            foreach (var header in headers)
                httpclient.DefaultRequestHeaders.Add(header.Key, header.Value);

        var jsondata = "";
        if (data != null) jsondata = JsonConvert.SerializeObject(data);
        var stringContent = new StringContent(jsondata,
            new MediaTypeHeaderValue("application/json"));

        return httpclient.PostAsync(url, stringContent);
    }

    public Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers = null)
    {
        var httpclient = GetDefaultClient();
        if (headers != null)
            foreach (var header in headers)
                httpclient.DefaultRequestHeaders.Add(header.Key, header.Value);

        return httpclient.GetAsync(url);
    }

    private HttpClient GetDefaultClient()
    {
        return _httpClientFactory.CreateClient(Global.DefaultHttpClientName);
    }
}