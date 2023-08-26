# AgileConfig_Client
AgileConfig的客户端，.net core standard2.0实现，core跟framework的.net程序都可以使用。
## 使用客户端
### 安装客户端
```
Install-Package AgileConfig.Client
```

☢️☢️☢️如果你的程序是Framework的程序请使用[AgileConfig.Client4FR](https://github.com/kklldog/AgileConfig.Client4FR)这个专门为Framework打造的client。使用当前版本有可能死锁造成cpu100% 的风险。☢️☢️☢️

### 初始化客户端
以asp.net core mvc项目为例：   
在appsettings.json文件内配置agileconfig的连接信息。
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",

  //agile_config
  "AgileConfig": {
    "appId": "app",
    "secret": "xxx",
    "nodes": "http://localhost:5000,http://localhost:5001"//多个节点使用逗号分隔,
    "name": "client1",
    "tag": "tag1",
    "env": "DEV",
    "httpTimeout": "100",
    "cache": {
      "directory": "agile/config"
    }
  }
}

```
#### 配置项说明

|配置项名称|配置项说明|是否必填|备注|
|--|--|--|--|
|appid|应用ID|是|对应后台管理中应用的`应用ID`|
|secret|应用密钥|是|对应后台管理中应用的`密钥`|
|nodes|应用配置节点|是|存在多个节点则使用逗号`,`分隔|
|name|连接客户端的自定义名称|否|方便在agile配置中心后台对当前客户端进行查阅与管理|
|tag|连接客户端自定义标签|否|方便在agile配置中心后台对当前客户端进行查阅与管理|
|env|配置中心的环境|否|通过此配置决定拉取哪个环境的配置信息；如果不配置，服务端会默认返回第一个环境的配置|
|cache|客户端的配置缓存设置|否|通过此配置可对拉取到本地的配置项文件进行相关设置|
|cache:directory|客户端的配置缓存文件存储地址配置|否|如设置了此目录则将拉取到的配置项cache文件存储到该目录，否则直接存储到站点根目录|
|httpTimeout|http请求超时时间|否|配置 client 发送 http 请求的时候的超时时间，默认100s|

## UseAgileConfig
在 program 类上使用 UseAgileConfig 扩展方法，该方法会配置一个 AgileConfig 的配置源。
```
 public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseAgileConfig(e => Console.WriteLine($"configs {e.Action}"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
```
## 读取配置
AgileConfig支持asp.net core 标准的IConfiguration，跟IOptions模式读取配置。还支持直接通过AgileConfigClient实例直接读取：
```
public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _IConfiguration;
        private readonly IOptions<DbConfigOptions> _dbOptions;
        private readonly IConfigClient _IConfigClient;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IOptions<DbConfigOptions> dbOptions, IConfigClient configClient)
        {
            _logger = logger;
            _IConfiguration = configuration;
            _dbOptions = dbOptions;
            _IConfigClient = configClient;
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

            return View();
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

            return View("ByInstance");
        }

        /// <summary>
        /// 使用Options模式读取配置
        /// </summary>
        /// <returns></returns>
        public IActionResult ByOptions()
        {
            var dbConn = _dbOptions.Value.connection;
            ViewBag.dbConn = dbConn;

            return View("ByOptions");
        }
    }
```
## 联系我
有什么问题可以mail我：minj.zhou@gmail.com
也可以加qq群：1022985150
