using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client
{
    class HttpUtil
    {
        public static HttpWebResponse Get(string url, Dictionary<string, string> headers, int? timeout)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }

            if (headers != null)
            {
                foreach (var keyValuePair in headers)
                {
                    request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            var response = request.GetResponse() as HttpWebResponse;

            return response;
        }

        public static async Task<HttpWebResponse> GetAsync(string url, Dictionary<string, string> headers, int? timeout)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }

            if (headers != null)
            {
                foreach (var keyValuePair in headers)
                {
                    request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            var response = (await request.GetResponseAsync()) as HttpWebResponse;

            return response;
        }

        public static async Task<HttpWebResponse> PostAsync(string url, Dictionary<string, string> headers, byte[] body, int? timeout, string contentType)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }

            if (headers != null)
            {
                foreach (var keyValuePair in headers)
                {
                    request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            request.ContentType = contentType;

            //add body
            if (body != null)
            {
                request.ContentLength = body.Length;
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(body, 0, body.Length);
                }
            }

            var response = (await request.GetResponseAsync()) as HttpWebResponse;

            return response;
        }

        public static async Task<HttpWebResponse> DeleteAsync(string url, Dictionary<string, string> headers, byte[] body, int? timeout, string contentType)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "DELETE";
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }

            if (headers != null)
            {
                foreach (var keyValuePair in headers)
                {
                    request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            request.ContentType = contentType;


            //add body
            if (body != null)
            {
                request.ContentLength = body.Length;
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(body, 0, body.Length);
                }
            }

            var response = (await request.GetResponseAsync()) as HttpWebResponse;

            return response;
        }

        public static async Task<string> GetResponseContentAsync(HttpWebResponse response)
        {
            using (var responseStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    return await reader.ReadToEndAsync();
            }
        }
    }
}
