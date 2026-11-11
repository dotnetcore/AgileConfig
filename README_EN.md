<p align="center">
    <img height="130" src="https://static.xbaby.xyz/blog/ac.png" style="height: 130px">
</p>
    
<h1 align="center">AgileConfig</h1>



[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![package workflow](https://github.com/dotnetcore/AgileConfig/actions/workflows/release-xxx.yml/badge.svg)](https://github.com/dotnetcore/AgileConfig/actions/workflows/release-xxx.yml)
![GitHub stars](https://img.shields.io/github/stars/dotnetcore/AgileConfig)
![Commit Date](https://img.shields.io/github/last-commit/kklldog/AgileConfig/master.svg?logo=github&logoColor=green&label=commit)
![Nuget](https://img.shields.io/nuget/v/agileconfig.client?label=agileconfig.client)
![Nuget](https://img.shields.io/nuget/dt/agileconfig.client?label=client%20download)
![Docker image](https://img.shields.io/docker/v/kklldog/agile_config?label=docker%20image)
![GitHub license](https://img.shields.io/github/license/kklldog/AgileConfig)
![build workflow](https://github.com/dotnetcore/AgileConfig/actions/workflows/master-ci.yml/badge.svg)
    


# [English](https://github.com/kklldog/AgileConfig/blob/master/README_EN.md) | [中文看这里](https://github.com/kklldog/AgileConfig/blob/master/README_CN.md)

This is a lightweight configuration center based on .net core . It is easy to deploy , easy to learn , easy to use .

CN Atomgit Mirror site: [AgileConfig](https://atomgit.com/dotnetcore/AgileConfig)

## 😍Features
1. easy to deploy (docker or IIS)
2. support distributed deploy
3. multiple environments support
4. configuration changes takes effect in real time
5. support IConfiguration , IOptions patten to read configurations
6. restful api
7. version management and easy to rollback
8. client fault tolerance
9. support OIDC/SSO
10. support OpenTelemetry
11. also can be use as a simple service register center
    
🔆🔆🔆Demo Project ：[AgileConfig Server Demo](http://agileconfig_server.xbaby.xyz)   name.pwd= admin/123456🔆🔆🔆   

Community Resource：   
.NET Client：[AgileConfig_Client](https://github.com/kklldog/AgileConfig_Client)   
JAVA Client：[AgileConfig_JClient](https://github.com/kklldog/agileconfig-jclient)   
GO Client：[AgileConfig_GoClient](https://github.com/kklldog/agileconfig_client_go)  
Python Client：[AgileConfig_GoClient](https://github.com/kklldog/agileconfig_client_python)  
GO Client：[go-agile-config](https://github.com/animacaeli/go-agile-config)  
Python Client：[py-agile-config](https://github.com/animacaeli/py-agile-config)   
Nodejs Client: [node-agile-client](https://github.com/LetTTGACO/node-agile-client)    
Rust Client: [agile-config-client](https://crates.io/crates/agile-config-client)  
Blazor UI：[AgileConfigBlazorUI](https://github.com/EzrealJ/AgileConfigBlazorUI)    

samples ：    
[AgileConfigMVCSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigMVCSample)   
[AgileConfig WPFSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigWPFSample)    
[AgileConfig ConsoleSample](https://github.com/kklldog/AgileConfig_Client/tree/master/AgileConfigConsoleSample)    

Q&A:   
https://github.com/kklldog/AgileConfig/wiki
 
API:   
[restful api](https://github.com/kklldog/AgileConfig/wiki/Restful-API)
## Release notes
[GitHub Releases](https://github.com/dotnetcore/AgileConfig/releases)
## architecture
![](https://s1.ax1x.com/2020/06/29/NRz1gO.png)
### client
A .net client to read configurations from server node .

### node
Node is just a .net core app . Client connect to the node in real time over websocket . Any node can be an admin console to manage configurations.
### database
AgileConfig support most popular databases.    
dbprovider :   
sqlserver = SqlServer   
mysql = MySql   
sqlite = Sqlite   
npgsql = PostgreSql   
oracle = Oracle  
mongodb = Mongodb

## ✅️How to use ? 
### run node on docker
``` shell
sudo docker run \
--name agile_config \
-e TZ=Asia/Shanghai \
-e adminConsole=true \
-e db__provider=sqlite \
-e db__conn="Data Source=agile_config.db" \
-p 5000:5000 \
-v /etc/localtime:/etc/localtime \
#-v /your_host_dir:/app/db \
-d kklldog/agile_config:latest
```

After the docker instance has successfully started you can visit http://localhost:5000 . 

#### Connect to MongoDB with a private CA (self-signed certificate)

When the MongoDB server certificate is signed by a private CA (for example a self-hosted replica set),
pass that CA to AgileConfig. Otherwise the driver fails with
`The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot`.

Two equivalent ways:

```shell
# 1) connection string option, the same form mongosh and the other official drivers accept
-e db__conn="mongodb://user:pwd@mdb1.example.com:27017/db?replicaSet=rs0&authSource=admin&tls=true&tlsCAFile=/certs/ca.crt" \
-v /host/path/ca.crt:/certs/ca.crt:ro \

# 2) dedicated setting, keeps the connection string portable across environments
-e db__tls__caFile=/certs/ca.crt \
-v /host/path/ca.crt:/certs/ca.crt:ro \
```

| Setting | Environment variable | Description |
|---|---|---|
| `db:tls:caFile` | `db__tls__caFile` | Path of the CA file (PEM or DER). A relative path resolves against the application directory. Mount the file into the container first. |
| `db:tls:ca` | `db__tls__ca` | Inline CA content: PEM (write line breaks as `\n`) or Base64 encoded DER. Useful when the certificate is injected as a secret instead of a file. |
| `db:tls:allowInvalidHostnames` | `db__tls__allowInvalidHostnames` | Skip the host name check only; chain validation still applies. |
| `db:tls:allowInvalidCertificates` | `db__tls__allowInvalidCertificates` | Skip certificate validation completely. Debug only. |
| `db:tls:checkCertificateRevocation` | `db__tls__checkCertificateRevocation` | Check CRL / OCSP while building the chain from `caFile` / `ca`. |

> - When none of them is set, the behavior is identical to previous versions (the OS trust store is used).
> - Per environment override: `db:env:{TEST|STAGING|PROD}:tls:*`; a missing key falls back to `db:tls:*`.
> - `tlsCAFile`, `tlsAllowInvalidCertificates` and `tlsAllowInvalidHostnames` (including the legacy `ssl` prefix) are recognized by AgileConfig and stripped from the connection string before it reaches the .NET driver, which does not support them natively. All other options are passed through.
> - Client certificates (mTLS, `tlsCertificateKeyFile`) are not supported yet; a clear error is returned.
> - The private CA is additional trust: certificates already trusted by the OS still work.

## use client
install client lib from nuget：
```
Install-Package AgileConfig.Client
```
add a section in appsettings.json of you application：
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
    "nodes": "http://localhost:5000,http://localhost:5001",
    "name": "client_name",
    "tag": "tag1",
    "env": "dev"
  }
}

```
in Main function add agileconfig client services:
``` c#
   public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseAgileConfig()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
```

## read configuration
You can still use `IConfiguration` or `IOption` patten to get the specific configuration value.
``` c#
public class HomeController : Controller
{
    private readonly IConfiguration _IConfiguration;
    private readonly IOptions<DbConfigOptions> _dbOptions;

    public HomeController(IConfiguration configuration, IOptions<DbConfigOptions> dbOptions)
    {
        _IConfiguration = configuration;
        _dbOptions = dbOptions;
    }
}
```

Or you can use `IConfigClient` interface to get the specific configuration value.

``` c#
public class HomeController : Controller
{
    private readonly IConfigClient _configClient

    public HomeController(IConfigClient configClient)
    {
        _configClient = configClient;
    }

    /// <summary>
    /// By IConfigClient
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
Or you can use a signleton instance without any injection.
``` c#
var userid = ConfigClient.Instance["userid"]
```

## donate
If this project is helpful to you, please scan the QR code below for a cup of coffee.    
 <img src="https://static.xbaby.xyz/alipay_qr.jpg|w30" width="300">
     
 <img src="https://static.xbaby.xyz/wechatpay_qr.jpg|w30" width="300">
   
## thanks 💖💖💖    
大鹏￥66.66 , 瘦草￥6.66 + 88 , ziana￥10.0 , Nullable￥9.99 , *三 ￥6.66 , HHM ￥6.66 , *。 ￥6.66 , 微笑刺客 ￥6.66 ,飞鸟与鱼 ￥38.88,  *航 ￥9.9, *啦 ￥6.66, *海 ￥6.66, Dyx 邓杨喜 ￥30 And more ...
## contact me
mail：minj.zhou@gmail.com   
🐧 group：1022985150

