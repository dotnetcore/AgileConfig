using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AgileConfigMVCSample.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using AgileConfig.Client.RegisterCenter;
using Newtonsoft.Json;

namespace AgileConfigMVCSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfigClient _IConfigClient;
        private readonly IConfiguration _IConfiguration;
        private readonly IOptions<DbConfigOptions> _dbOptions;
        private readonly IOptionsSnapshot<DbConfigOptions> _dbOptionsSnapshot;
        private readonly IOptionsMonitor<DbConfigOptions> _dbOptionsMonitor;
        private readonly IDiscoveryService _discoveryService;

        public HomeController(
            ILogger<HomeController> logger, 
            IConfiguration configuration, 
            IOptions<DbConfigOptions> dbOptions, 
            IOptionsSnapshot<DbConfigOptions> dbOptionsSnapshot,
            IOptionsMonitor<DbConfigOptions> dbOptionsMonitor,
            IConfigClient configClient,
            IDiscoveryService discoveryService)
        {
            _logger = logger;
            _IConfigClient = configClient;
            _IConfiguration = configuration;
            _dbOptions = dbOptions;
            _dbOptionsSnapshot = dbOptionsSnapshot;
            _dbOptionsMonitor = dbOptionsMonitor;

            _dbOptionsMonitor.OnChange((o, s) => {
                Console.WriteLine(o.connection);
                Console.WriteLine(s);
            });

            _discoveryService = discoveryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 使用IConfiguration读取配置
        /// </summary>
        /// <returns></returns>
        public IActionResult ByIConfiguration()
        {
            var userId = _IConfiguration["userId"];
            var dbConn = _IConfiguration["db:connection"];

            ViewBag.userId = userId;
            ViewBag.dbConn = dbConn;

            return View("Configuration");
        }

        /// <summary>
        /// 直接使用ConfigClient的实例读取配置
        /// </summary>
        /// <returns></returns>
        public IActionResult ByInstance()
        {
            var userId = _IConfigClient["userId"];
            var dbConn = _IConfigClient["db:connection"];

            ViewBag.userId = userId;
            ViewBag.dbConn = dbConn;

            return View("Configuration");
        }

        /// <summary>
        /// 使用Options模式读取配置
        /// </summary>
        /// <returns></returns>
        public IActionResult ByOptions()
        {
            var dbConn = _dbOptions.Value.connection;
            ViewBag.dbConn = dbConn;

            return View("Option");
        }

        public IActionResult ByOptionsSnapshot()
        {
            var dbConn = _dbOptionsSnapshot.Value.connection;
            ViewBag.dbConn = dbConn;

            return View("Option");
        }

        public IActionResult ByOptionsMonitor()
        {
            var dbConn = _dbOptionsMonitor.CurrentValue.connection;
            ViewBag.dbConn = dbConn;

            return View("Option");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Health()
        {
            return Content("ok");
        }

        public IActionResult Services(string serviceName)
        {
            var services = _discoveryService.Services;

            if (serviceName != null)
            {
                services = _discoveryService.GetByServiceName(serviceName).ToList();
            }

            var json = JsonConvert.SerializeObject(services);
            ViewBag.services = json;

            return View("Services");
        }

        public IActionResult ServiceDown([FromBody] ServiceDownAlarmMessageVM message)
        {
            Console.WriteLine(message.Message);

            return Json(message);
        }
    }
}
