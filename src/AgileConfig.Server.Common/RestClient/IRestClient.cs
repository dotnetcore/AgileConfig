using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common.RestClient;

public interface IRestClient
{
    Task<T> GetAsync<T>(string url, Dictionary<string, string> headers = null);

    Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers = null);

    Task<T> PostAsync<T>(string url, object data, Dictionary<string, string> headers = null);

    Task<HttpResponseMessage> PostAsync(string url, object data, Dictionary<string, string> headers = null);
}