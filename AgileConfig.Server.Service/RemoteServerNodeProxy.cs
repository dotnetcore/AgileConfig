using Agile.Config.Protocol;
using AgileConfig.Server.IService;
using AgileHttp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class RemoteServerNodeProxy : IRemoteServerNodeProxy
    {
        public async Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action)
        {
            using (var resp = await (address + "/RemoteOP/AllClientsDoAction")
                                    .AsHttp("POST", action)
                                    .Config(new RequestOptions { ContentType = "application/json" })
                                    .SendAsync())
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = resp.Deserialize<dynamic>();

                    if ((bool)result.success)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public async Task<bool> AppClientsDoActionAsync(string address, string appId, WebsocketAction action)
        {
            using (var resp = await (address + "/RemoteOP/AppClientsDoAction".AppendQueryString("appId", appId))
                                    .AsHttp("POST", action)
                                    .Config(new RequestOptions { ContentType = "application/json" })
                                    .SendAsync())
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = resp.Deserialize<dynamic>();

                    if ((bool)result.success)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public async Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action)
        {
            using (var resp = await (address + "/RemoteOP/OneClientDoAction?clientId=" + clientId)
                                    .AsHttp("POST", action)
                                    .Config(new RequestOptions { ContentType = "application/json" })
                                    .SendAsync())
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = resp.Deserialize<dynamic>();

                    if ((bool)result.success)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
