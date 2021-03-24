using System;
using Agile.Config.Protocol;
using AgileConfig.Server.Apisite.Websocket;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    /// <summary>
    /// 这个Controller用来接受控制台节点发送过来的命令
    /// </summary>
    public class RemoteOPController : Controller
    {
        [HttpPost]
        public IActionResult AllClientsDoActionAsync([FromBody]WebsocketAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            WebsocketCollection.Instance.SendActionToAll(action);

            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public IActionResult AppClientsDoActionAsync([FromQuery]string appId, [FromBody]WebsocketAction action)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            WebsocketCollection.Instance.SendActionToAppClients(appId, action);

            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public IActionResult OneClientDoActionAsync([FromQuery]string clientId, [FromBody]WebsocketAction action)
        {
            var client = WebsocketCollection.Instance.Get(clientId);
            if (client == null)
            {
                throw new Exception($"Can not find websocket client by id: {clientId}");
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            WebsocketCollection.Instance.SendActionToOne(client, action);

            return Json(new
            {
                success = true,
            });
        }

    }
}
