<p align="center">
    <img height="130" src="https://ftp.bmp.ovh/imgs/2021/04/5162f8595d9c6a99.png" style="height: 130px">
</p>
    
<h1 align="center">AgileConfig</h1>




![GitHub Workflow Status](https://img.shields.io/github/workflow/status/kklldog/agileconfig/.NET%20Core)
![GitHub stars](https://img.shields.io/github/stars/kklldog/AgileConfig)
![Commit Date](https://img.shields.io/github/last-commit/kklldog/AgileConfig/master.svg?logo=github&logoColor=green&label=commit)
![Nuget](https://img.shields.io/nuget/v/agileconfig.client?label=agileconfig.client)
![Nuget](https://img.shields.io/nuget/dt/agileconfig.client?label=client%20download)
![Docker image](https://img.shields.io/docker/v/kklldog/agile_config?label=docker%20image)
![GitHub license](https://img.shields.io/github/license/kklldog/AgileConfig)

    


# [English](https://github.com/kklldog/AgileConfig/blob/master/README.md) | [中文](https://github.com/kklldog/AgileConfig/blob/master/README_CN.md)

This is a lightweight configuration center based on .net core . It is easy to deploy , easy to learn , easy to use .
## Features
1. easy to deploy (docker or IIS)
2. support distributed deploy
3. configuration changes takes effect in real time
4. support IConfiguration , IOptions patten to read configurations
5. restful api
6. version management
    
Demo Project ：[AgileConfig Server Demo](http://agileconfig.xbaby.xyz:5000)   password ：123456   
client project ：[AgileConfig_Client](https://github.com/kklldog/AgileConfig_Client)   
samples ：    
[AgileConfigMVCSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigMVCSample)   
[AgileConfig WPFSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigWPFSample)    
[AgileConfig ConsoleSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigConsoleSample)    
 
API:   
[restful api](https://github.com/kklldog/AgileConfig/wiki/Restful-API)
## ChangeLog
[Changelog](https://github.com/kklldog/AgileConfig/blob/master/CHANGELOG.md)
## architecture
![](https://s1.ax1x.com/2020/06/29/NRz1gO.png)
### client
A .net client to read configurations from server node .
### node
Node is just a .net core app . Client connect to the node to read configurations . Any node can be an admin console to manage configuration .
### database
AgileConfig can use many database to persist configurations .    
dbprovider : 
sqlserver = SqlServer   
mysql = MySql   
sqlite = Sqlite   
npgsql = PostgreSql   
oracle = Oracle  

## useage
### run node
```
sudo docker run --name agile_config -e adminConsole=true -e db:provider=sqlite -e db:conn="Data Source=agile_config.db" -p 5000:5000 -v /etc/localtime:/etc/localtime  kklldog/agile_config:latest
```
## use client
install client lib from nuget：
```
Install-Package AgileConfig.Client -Version 1.1.8.4
```
add a agileconfig section in appsettings.json：
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
    "nodes": "http://localhost:5000,http://localhost:5001",
    "name": "client_name",
    "tag": "tag1"
  }
}

```
```
       public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
        
                config.AddAgileConfig();
            })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


```
## read configuration
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
        /// By IConfiguration to read
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

    }
```
## screenshots
![](https://ftp.bmp.ovh/imgs/2021/04/44242b327230c5e6.png)   
![](https://ftp.bmp.ovh/imgs/2021/04/7e93011590c55d12.png)   
![](https://ftp.bmp.ovh/imgs/2021/04/a48014f02ced6804.png)    
![](https://ftp.bmp.ovh/imgs/2021/04/8ae7d8bfcef72518.png)   
![](https://ftp.bmp.ovh/imgs/2021/04/74fbc7f1daab5deb.png)   
![](https://ftp.bmp.ovh/imgs/2021/04/9f38d55804e858d5.png)   

## donate
If this project is helpful to you, please scan the QR code below for a cup of coffee.    
<img src = 'https://ftp.bmp.ovh/imgs/2021/04/c0146fa995e8074d.jpg'  width="300"/>    
<img src = 'https://ftp.bmp.ovh/imgs/2021/04/1c8748c5732b8fbe.jpg' width="300"/>
   
thanks for    
大鹏￥66.66 , 瘦草￥6.66
## contact me
mail：minj.zhou@gmail.com   
qq group：1022985150

