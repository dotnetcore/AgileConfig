using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AgileConfigMVCSample.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AgileConfigMVCSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _IConfiguration;
        private readonly IOptions<DbConfigOptions> _dbOptions;
        private readonly IOptionsSnapshot<DbConfigOptions> _dbOptionsSnapshot;
        private readonly IOptionsMonitor<DbConfigOptions> _dbOptionsMonitor;

        public HomeController(
            ILogger<HomeController> logger, 
            IConfiguration configuration, 
            IOptions<DbConfigOptions> dbOptions, 
            IOptionsSnapshot<DbConfigOptions> dbOptionsSnapshot,
            IOptionsMonitor<DbConfigOptions> dbOptionsMonitor)
        {
            _logger = logger;
            _IConfiguration = configuration;
            _dbOptions = dbOptions;
            _dbOptionsSnapshot = dbOptionsSnapshot;
            _dbOptionsMonitor = dbOptionsMonitor;

            _dbOptionsMonitor.OnChange((o, s) => {
                Console.WriteLine(o.connection);
                Console.WriteLine(s);
            });
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
            var userId = Program.ConfigClient["userId"];
            var dbConn = Program.ConfigClient["db:connection"];

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

            return View("Configuration");
        }

        public IActionResult ByOptionsSnapshot()
        {
            var dbConn = _dbOptionsSnapshot.Value.connection;
            ViewBag.dbConn = dbConn;

            return View("Configuration");
        }

        public IActionResult ByOptionsMonitor()
        {
            var dbConn = _dbOptionsMonitor.CurrentValue.connection;
            ViewBag.dbConn = dbConn;

            return View("Configuration");
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
    }
}
