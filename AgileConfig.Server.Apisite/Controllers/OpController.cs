using System;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Apisite.Websocket;
using AgileHttp;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class OPController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Client_Offline(string clientId)
        {
            var client = WebsocketCollection.Instance.Get(clientId);
            if (client == null)
            {
                throw new Exception($"Can not find websocket client by id: {clientId}");
            }
            await WebsocketCollection.Instance.SendActionToOne(client, new WebsocketAction { Action = "offline" });

            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoteClient_Offline(string address, string clientId)
        {
            using (var resp = await (address + "/op/Client_Offline?clientId=" + clientId).AsHttp("POST").SendAsync())
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = resp.Deserialize<dynamic>();

                    if ((bool)result.success)
                    {
                        return Json(new
                        {
                            success = true,
                        });
                    }
                }

                return Json(new
                {
                    success = false,
                });
            }
        }

        [HttpPost]
        public IActionResult AllClientsReload()
        {
            WebsocketCollection.Instance.SendActionToAll(new WebsocketAction { Action = "reload" });
            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoteServerNode_AllClientReload(string address)
        {
            using (var resp = await (address + "/op/AllClientsReload").AsHttp("POST").SendAsync())
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = resp.Deserialize<dynamic>();

                    if ((bool)result.success)
                    {
                        return Json(new
                        {
                            success = true,
                        });
                    }
                }

                return Json(new
                {
                    success = false,
                });
            }
        }
    }
}
