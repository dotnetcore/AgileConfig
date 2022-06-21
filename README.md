<p align="center">
    <img height="130" src="https://ftp.bmp.ovh/imgs/2021/04/5162f8595d9c6a99.png" style="height: 130px">
</p>
    
<h1 align="center">AgileConfig</h1>




[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![package workflow](https://github.com/dotnetcore/AgileConfig/actions/workflows/release-xxx.yml/badge.svg)](https://github.com/dotnetcore/AgileConfig/actions/workflows/release-xxx.yml)
![GitHub stars](https://img.shields.io/github/stars/kklldog/AgileConfig)
![Commit Date](https://img.shields.io/github/last-commit/kklldog/AgileConfig/master.svg?logo=github&logoColor=green&label=commit)
![Nuget](https://img.shields.io/nuget/v/agileconfig.client?label=agileconfig.client)
![Nuget](https://img.shields.io/nuget/dt/agileconfig.client?label=client%20download)
![Docker image](https://img.shields.io/docker/v/kklldog/agile_config?label=docker%20image)
![GitHub license](https://img.shields.io/github/license/kklldog/AgileConfig)
    
# [English](https://github.com/kklldog/AgileConfig/blob/master/README_EN.md) | [中文](https://github.com/kklldog/AgileConfig/blob/master/README.md)
    
这是一个基于.net core开发的轻量级配置中心。说起配置中心很容易让人跟微服务联系起来，如果你选择微服务架构，那么几乎逃不了需要一个配置中心。事实上我这里并不是要蹭微服务的热度。这个世界上有很多分布式程序但它并不是微服务。比如有很多传统的SOA的应用他们分布式部署，但并不是完整的微服务架构。这些程序由于分散在多个服务器上所以更改配置很困难。又或者某些程序即使不是分布式部署的，但是他们采用了容器化部署，他们修改配置同样很费劲。所以我开发AgileConfig并不是为了什么微服务，我更多的是为了那些分布式、容器化部署的应用能够更加简单的读取、修改配置。    
AgileConfig秉承轻量化的特点，部署简单、配置简单、使用简单、学习简单，它只提取了必要的一些功能，并没有像Apollo那样复杂且庞大。但是它的功能也已经足够你替换webconfig，appsettings.json这些文件了。如果你不想用微服务全家桶，不想为了部署一个配置中心而需要看N篇教程跟几台服务器那么你可以试试AgileConfig  ：）   
    
Gitee 镜像：[https://gitee.com/kklldog/AgileConfig](https://gitee.com/kklldog/AgileConfig)    

演示地址：[AgileConfig Server Demo](http://agileconfig_server.xbaby.xyz)   用户名：admin 密码：123456   
.NET 客户端项目：[AgileConfig_Client](https://github.com/kklldog/AgileConfig_Client)   

示例项目：    
[AgileConfig MVCSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigMVCSample)   
[AgileConfig WPFSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigWPFSample)    
[AgileConfig ConsoleSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigConsoleSample)    
[教程 - 如何使用AgileConfig.Client读取配置](https://www.cnblogs.com/kklldog/p/how-to-use-agileconfigclient.html)    
[教程- 如何使用服务注册与发现](https://www.cnblogs.com/kklldog/p/agileconfig-160.html)   
💥💥[常见问题，必看！！！](https://github.com/kklldog/AgileConfig/wiki/%E5%B8%B8%E8%A7%81%E9%97%AE%E9%A2%98%EF%BC%8C%E5%BF%85%E7%9C%8B%EF%BC%81%EF%BC%81%EF%BC%81)  
[关于多环境的部署说明](https://github.com/dotnetcore/AgileConfig/wiki/%E5%85%B3%E4%BA%8E-1.5-%E7%89%88%E6%9C%AC%E6%94%AF%E6%8C%81%E5%A4%9A%E7%8E%AF%E5%A2%83%E7%9A%84%E8%AF%B4%E6%98%8E)   
    
社区资源：   
🌷 基于 Nodejs 实现的客户端: [node-agile-client](https://github.com/LetTTGACO/node-agile-client)    
🌷 基于 Blazor 实现的管理界面：[AgileConfigBlazorUI](https://github.com/EzrealJ/AgileConfigBlazorUI)   
   
Restful API:   
[✈️ restful api](https://github.com/kklldog/AgileConfig/wiki/Restful-API)
   
## ChangeLog
[📝 Changelog](https://github.com/kklldog/AgileConfig/blob/master/CHANGELOG.md)   
   
## 联系作者
[📧 联系](#联系我)
## 打赏
[❤️❤️❤️ 打赏](#如果觉得这个项目对你有帮助可以给作者早餐加个蛋)
## 特点
1. 部署简单，最少只需要一个数据节点，支持docker部署
2. 支持多节点分布式部署来保证高可用
3. 配置支持按应用隔离，应用内配置支持分组隔离
4. 支持多环境
5. 应用支持继承，可以把公共配置提取到一个应用然后其它应用继承它
6. 使用长连接技术，配置信息实时推送至客户端
7. 支持IConfiguration，IOptions模式读取配置，原程序几乎可以不用改造
8. 配置修改支持版本记录，随时回滚配置
9. 如果所有节点都故障，客户端支持从本地缓存读取配置
10. 支持Restful API维护配置
11. v-1.6.0 以上已支持服务注册与发现

    
## 💥 务必在使用 AgileConfig 之前仔细阅读以下文档
## 架构
![](https://s1.ax1x.com/2020/06/29/NRz1gO.png)
AgileConfig的架构比较简单，主要是分3块：
### 客户端
客户端程序是使用netstandard2.0开发的一个类库，方便.net core程序接入，nuget搜agileconfig.client就可以安装。可以在启动客户端的时候配置多个节点的地址，客户端会随机挑选一个进行连接，连接成功后会维持一个websocket长连接。如果连接的节点发生故障导致连接中断，客户端会继续随机一个节点进行连接，直到连接成功。
### 节点、管理程序
节点是使用asp.net core开发的一个服务。为了部署简单，直接把管理程序跟节点服务合二为一了。任何一个节点都可以在启动的时候配置环境变量开启管理程序功能。
### 数据库
使用数据库来存储数据，目前支持Sqlserver, Mysql, Sqlite, PostgreSql,Oracle 五种数据库。最新版本已经切换为Freesql为数据访问组件。Freesql对多数据库的支持更加强劲，特别是对国产数据库的支持。但是因为没有国产数据库的测试环境，本项目并未支持，如果有需要我可是开分支尝试支持，但是测试工作就要靠用户啦。
### 关于高可用
AgileConfig的节点都是无状态的，所以可以横向部署多个节点来防止单点故障。在客户端配置多个节点地址后，客户端会随机连接至某个节点。
| 问题 | 影响 | 说明 |   
| ---- | ---- | ---- |   
| 控制台下线 | 无法维护配置，客户端无影响 | 因为控制台跟节点是共存的，所以某个控制台下线一般来说同样意味着一个节点的下线 |   
| 某个节点下线 | 客户端重连至其他节点 | 无任何影响 |    
| 所有节点下线 | 客户端从内存读取配置 | 启动的客户端会从内存读取配置，未启动的客户端会再尝试连接到节点多次失败后，尝试从本地文件缓存读取配置，保证应用可以启动 |    
   
有同学说你这样没什么卵用，数据库还是单点的，一旦数据库崩了，同样GG。但是数据库有数据库的高可用技术，比如mysql的binlog等等。至于数据库的高可用还是让数据库自己搞定吧。从架构上看携程的apollo数据库也是单点的。
## 部署服务端
## 初始化数据库
用户只需要手工建一个空库，所有的表在第一次启动的时候都会自动生成。目前支持sqlserver，mysql，sqlite, PostgreSql，Oracle 五种数据库。
provider对照：   
sqlserver = SqlServer   
mysql = MySql   
sqlite = Sqlite   
npgsql = PostgreSql   
oracle = Oracle   
## 使用服务端
### 运行服务端
使用 docker 运行
``` shell
sudo docker run \
--name agile_config \
-e TZ=Asia/Shanghai \
-e adminConsole=true \
-e db:provider=sqlite \
-e db:conn="Data Source=agile_config.db" \
-p 5000:5000 \
#-v /your_host_dir:/app/db \
-d kklldog/agile_config:latest
```
通过docker建立一个agile_config实例，其中有3个环境变量需要配置:    
1. adminConsole 配置程序是否为管理控制台。如果为true则启用控制台功能，访问该实例会出现管理界面。
2. db:provider 配置程序的数据库类型。目前程序支持：sqlserver，mysql，sqlite, PostgreSql，Oracle 五种数据库。
3. db:conn 配置数据库连接串    
   
> 💥注意：如果通过IIS或者别的方式部署，请自行从主页上的[releases](https://github.com/dotnetcore/AgileConfig/releases)页面下载最新的部署包。如果自己使用源码编译，请先编译react-ui-antd项目把dist内的产物复制到apisite项目的wwwroot/ui目录下。调试的时候需要复制到bin目录下。

使用 docker-compose 运行多节点集群, 环境变量 cluster=true 会尝试获取容器的 IP ，主动注册到节点列表：   
```
version: '3'
services:
  agile_config_admin:
    image: "kklldog/agile_config"
    ports:
      - "15000:5000"
    networks:
      - net0
    volumes:
      - /etc/localtime:/etc/localtime
    environment:
      - TZ=Asia/Shanghai
      - adminConsole=true
      - cluster=true
      - db:provider=mysql
      - db:conn= database=configcenter;data source=192.168.0.115;User Id=root;password=mdsd;port=3306
  agile_config_node1:
    image: "kklldog/agile_config"
    ports:
      - "15001:5000"
    networks:
      - net0
    volumes:
      - /etc/localtime:/etc/localtime
    environment:
      - TZ=Asia/Shanghai
      - cluster=true
      - db:provider=mysql
      - db:conn= database=configcenter;data source=192.168.0.115;User Id=root;password=mdsd;port=3306
    depends_on:
      - agile_config_admin
  agile_config_node2:
    image: "kklldog/agile_config"
    ports:
      - "15002:5000"
    networks:
      - net0
    volumes:
      - /etc/localtime:/etc/localtime
    environment:
      - TZ=Asia/Shanghai
      - cluster=true
      - db:provider=mysql
      - db:conn= database=configcenter;data source=192.168.0.115;User Id=root;password=mdsd;port=3306
    depends_on:
      - agile_config_admin
networks:
  net0:
```
### 初始化管理员密码
第一次运行程序需要初始化超级管理员密码，超管用户名固定为 admin    
![](https://ftp.bmp.ovh/imgs/2021/04/44242b327230c5e6.png)
### 节点
AgileConfig支持多节点部署，所有的节点都是平行的。为了简化部署，AgileConfig并没有单独的控制台程序，请直接使用任意一个节点作为控制台。当环境变量adminConsole=true时，该节点同时兼备数据节点跟控制台功能。为了控制台能够管理节点，所以需要在控制台配置节点的信息。
> 💥注意：即使是作为控制台的数据节点同样需要添加到管理程序，以便管理它。
    
![](https://ftp.bmp.ovh/imgs/2021/04/7e93011590c55d12.png)
### 应用
AgileConfig支持多应用程序接入。需要为每个应用程序配置名称、ID、秘钥等信息。    
每个应用可以设置是否可以被继承，可以被继承的应用类似apollo的公共 namespace 的概念。公共的配置可以提取到可继承应用中，其它应用只要继承它就可以获得所有配置。   
如果子应用跟被继承应用之间的配置键发生重复，子应用的配置会覆盖被继承的应用的配置。子应用可以继承多个应用，如果多个应用之间发生重复键，按照继承的顺序，后继承的应用的配置覆盖前面的应用。
    
![](https://ftp.bmp.ovh/imgs/2021/04/a48014f02ced6804.png)
### 配置项
配置完应用信息后可以为每个应用配置配置项。配置项支持分组。新添加的配置并不会被客户端感知到，需要手工点击“发布”才会推送给客户端。已上线的配置如果发生修改、删除、回滚操作，会实时推送给客户端。版本历史记录了配置的历史信息，可以回滚至任意版本。
    
![](https://ftp.bmp.ovh/imgs/2021/04/8ae7d8bfcef72518.png)
### 客户端
控制台可以查看已连接的客户端。
    
![](https://ftp.bmp.ovh/imgs/2021/04/74fbc7f1daab5deb.png)
### 系统日志
系统日志记录了AgileConfig生产中的一些关键信息。
    
![](https://ftp.bmp.ovh/imgs/2021/04/9f38d55804e858d5.png)

## 使用客户端
客户端[AgileConfig_Client](https://github.com/kklldog/AgileConfig_Client)是使用.net core standard2.0编写的一个类库，已发布到nuget，方便用户集成。
### 使用nuget安装客户端类库
```
Install-Package AgileConfig.Client
```
### 初始化客户端
以asp.net core mvc项目为例：
``` json
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
    "name": "client_name",
    "tag": "tag1",
    "env": "DEV"
  }
}

```
在appsettings.json文件配置agileconfig的配置信息。
``` c#
     public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseAgileConfig(e => Console.WriteLine($"configs {e.Action}"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
```
使用 UseAgileConfig 扩展方法配置一个配置源。
``` c#
     public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseAgileConfig(new ConfigClient($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json"), e => Console.WriteLine($"configs {e.Action}"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
```
如果需要根据环境变量读取appsettings.{env}.json配置信息，可以通过Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")来获取。   
> 💥注意：如果你的程序是Framework的程序请使用[AgileConfig.Client4FR](https://github.com/kklldog/AgileConfig.Client4FR)这个专门为Framework打造的client。使用当前版本有可能死锁造成cpu100% 的风险。

> 💥注意：如果节点使用nginx反代的话，需要对nginx进行配置，使其支持websocket协议，不然客户端跟节点的长连接没法建立。

## 读取配置
AgileConfig支持asp.net core 标准的IConfiguration，跟IOptions模式读取配置。
``` c#
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

也可以通过IConfigClient来获取这个实例

``` c#
public class HomeController : Controller
{
    private readonly IConfigClient _configClient

    public HomeController(IConfigClient configClient)
    {
        _configClient = configClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// 使用IConfigClient读取配置
    /// </summary>
    /// <returns></returns>
    public IActionResult ByIConfigClient()
    {
        var userId = _configClient["userId"];
        var dbConn = _configClient["db:connection"];

        foreach (var item in _configClient.Data)
        {
            Console.WriteLine($"{item.Key} = {item.Value}");
        }

        ViewBag.userId = userId;
        ViewBag.dbConn = dbConn;

        return View();
    }
}
```

## 联系我
有什么问题可以mail我：minj.zhou@gmail.com   
也可以加qq群：1022985150
## 如果觉得这个项目对你有帮助可以给作者早餐加个蛋🍳🍳🍳
 <img src="https://static.xbaby.xyz/alipay_qr.jpg|w30" width="300">
     
 <img src="https://static.xbaby.xyz/wechatpay_qr.jpg|w30" width="300">


        
### 感谢💖💖💖
大鹏￥66.66 , 瘦草￥6.66 + 88 , ziana￥10.0 , Nullable￥9.99 , *三 ￥6.66 , HHM ￥6.66 , 微笑刺客 ￥6.66 , 飞鸟与鱼 ￥38.88, *航 ￥9.9, *啦 ￥6.66, *海 ￥6.66, Dyx 邓杨喜 ￥30
