using System;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite.Controllers
{
    /// <summary>
    /// 这个Controller用来接受其它节点发送过来的命令
    /// </summary>
    public class RemoteOPController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IServiceInfoService _serviceInfoService;
        private readonly ILogger _logger;
        public RemoteOPController(IConfigService configService,
            IServiceInfoService serviceInfoService,
            ILoggerFactory loggerFactory)
        {
            _serviceInfoService = serviceInfoService;
            _configService = configService;
            _logger = loggerFactory.CreateLogger<RemoteServerProxyController>();
        }
        
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
        public IActionResult AppClientsDoActionAsync([FromQuery]string appId,[FromQuery]string env, [FromBody]WebsocketAction action)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (string.IsNullOrEmpty(env))
            {
                throw new ArgumentNullException(nameof(env));
            }

            WebsocketCollection.Instance.SendActionToAppClients(appId, env, action);

            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> OneClientDoActionAsync([FromQuery]string clientId, [FromBody]WebsocketAction action)
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

            await WebsocketCollection.Instance.SendActionToOne(client, action);

            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public IActionResult ClearConfigServiceCache()
        {
            _configService.ClearCache();

            _logger.LogInformation("Server clear all config's cache .");
            
            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public IActionResult ClearServiceInfoCache()
        {
            _serviceInfoService.ClearCache();

            _logger.LogInformation("Server clear all serviceInfo's cache .");
            
            return Json(new
            {
                success = true,
            });
        }

        [HttpPost]
        public IActionResult RegisterNode()
        {
            return Content("ok");
        }
    }
}
