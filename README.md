# AgileConfig
这是一个基于.net core开发的轻量级配置中心。
1. 部署简单，最少只需要一个数据节点，支持docker部署
2. 支持多节点分布式部署来保证高可用
3. 配置支持按应用隔离，应用内配置支持分组隔离
4. 使用长连接技术，配置信息实时推送至客户端
5. 支持IConfiguration，IOptions模式读取配置，原程序几乎可以不用改造
6. 配置修改支持版本记录，随时回滚配置
7. 如果所有节点都故障，客户端支持从本地缓存读取配置
## 架构
![](https://s1.ax1x.com/2020/06/29/NRz1gO.png)
AgileConfig的架构比较简单，主要是分3块：
### 客户端
客户端程序是使用netstandard2.0开发的一个类库，方便.net core程序接入，nuget搜agileconfig.client就可以安装。可以在启动客户端的时候配置多个节点的地址，客户端会随机挑选一个进行连接，连接成功后会维持一个websocket长连接。如果连接的节点发生故障导致连接中断，客户端会继续随机一个节点进行连接，直到连接成功。
### 节点、管理程序
节点是使用asp.net core开发的一个服务。为了部署简单，直接把管理程序跟节点服务合二为一了。任何一个节点都可以在启动的时候配置环境变量开启管理程序功能。
### 数据库
使用数据库来存储数据，目前支持Sqlserver, Mysql, Sqlite, PostgreSql 四种数据库。因为服务端使用EF Core框架访问数据，原则上只要EF Core支持的数据库，节点就可以很方便的支持它。

## 部署服务端
## 初始化数据库
用户只需要手工建一个空库，所有的表在第一次启动的时候都会自动生成。目前支持sqlserver，mysql，sqlite, PostgreSql 四种数据库。
provider对照：   
sqlserver = SqlServer   
mysql = MySql   
sqlite = Sqlite   
npgsql = PostgreSql   
## 使用服务端
### 运行服务端
```
sudo docker run --name agile_config -e adminConsole=true -e db:provider=sqlite -e db:conn="Data Source=agile_config.db" -p 5000:5000 kklldog/agile_config:latest
```
通过docker建立一个agile_config实例，其中有3个环境变量需要配置:    
1. adminConsole 配置程序是否为管理控制台。如果为true则启用控制台功能，访问该实例会出现管理界面。
2. db:provider 配置程序的数据库类型。目前程序支持：sqlite，mysql，sqlserver 三种数据库。
3. db:conn 配置数据库连接串    
    
![](https://s1.ax1x.com/2020/06/09/t4rDfA.png)
### 初始化管理员密码
第一次远行程序需要初始化管理员密码    
![](https://s1.ax1x.com/2020/06/09/t4DgIJ.png)
### 节点
AgileConfig支持多节点部署，所有的节点都是平行的。为了简化部署，AgileConfig并没有单独的控制台程序，请直接使用任意一个节点作为控制台。当环境变量adminConsole=true时，该节点同时兼备数据节点跟控制台功能。为了控制台能够管理节点，所以需要在控制台配置节点的信息。
> 注意：即使是作为控制台的数据节点同样需要添加到管理程序，以便管理它。
    
![](https://s1.ax1x.com/2020/06/09/t4DxQP.png)
### 应用
AgileConfig支持多应用程序接入。需要为每个应用程序配置名称、ID、秘钥等信息。
    
![](https://s1.ax1x.com/2020/06/09/t4rSL8.png)
### 配置项
配置完应用信息后可以为每个应用配置配置项。配置项支持分组。新添加的配置并不会被客户端感知到，需要手工点击“上线”才会推送给客户端。已上线的配置如果发生修改、删除、回滚操作，会实时推送给客户端。版本历史记录了配置的历史信息，可以回滚至任意版本。
    
![](https://s1.ax1x.com/2020/06/09/t4rFij.png)
### 客户端
控制台可以查看已连接的客户端。
    
![](https://s1.ax1x.com/2020/06/09/t4rmLT.png)
### 系统日志
系统日志记录了AgileConfig生产中的一些关键信息。
    
![](https://s1.ax1x.com/2020/06/09/t4rYy6.png)

## 使用客户端
客户端[AgileConfig_Client](https://github.com/kklldog/AgileConfig_Client)是使用.net core standard2.0编写的一个类库，已发布到nuget，方便用户集成。
### 使用nuget安装客户端类库
```
Install-Package AgileConfig.Client
```
### 初始化客户端
以asp.net core mvc项目为例：
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
    "nodes": "http://localhost:5000,http://localhost:5001"//多个节点使用逗号分隔
  }
}

```
```
       public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                //读取本地配置
                var localconfig = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json").Build();
                //从本地配置里读取AgileConfig的相关信息
                var appId = localconfig["AgileConfig:appId"];
                var secret = localconfig["AgileConfig:secret"];
                var nodes = localconfig["AgileConfig:nodes"];
                //new一个client实例
                var configClient = new ConfigClient(appId, secret, nodes);
                //使用AddAgileConfig配置一个新的IConfigurationSource
                config.AddAgileConfig(configClient);
                //找一个变量挂载client实例，以便其他地方可以直接使用实例访问配置
                ConfigClient = configClient;
                //注册配置项修改事件
                configClient.ConfigChanged += ConfigClient_ConfigChanged;
            })
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

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IOptions<DbConfigOptions> dbOptions)
        {
            _logger = logger;
            _IConfiguration = configuration;
            _dbOptions = dbOptions;
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
            var userId = Program.ConfigClient["userId"];
            var dbConn = Program.ConfigClient["db:connection"];

            ViewBag.userId = userId;
            ViewBag.dbConn = dbConn;

            return View("ByIConfiguration");
        }

        /// <summary>
        /// 使用Options模式读取配置
        /// </summary>
        /// <returns></returns>
        public IActionResult ByOptions()
        {
            var dbConn = _dbOptions.Value.connection;
            ViewBag.dbConn = dbConn;

            return View("ByIConfiguration");
        }
    }
```
