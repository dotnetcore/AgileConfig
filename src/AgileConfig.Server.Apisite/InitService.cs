using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite
{
    public class InitService : IHostedService
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IEventRegister _eventRegister;
        private readonly IServerNodeService _serverNodeService;
        private readonly IServiceHealthCheckService _serviceHealthCheckService;
        private readonly ISystemInitializationService _systemInitializationService;
        private readonly ILogger _logger;
        private readonly IServiceScope _localServiceScope;
        public InitService(IServiceScopeFactory serviceScopeFactory, ISystemInitializationService systemInitializationService, ILogger<InitService> logger)
        {
            _logger = logger;
            _systemInitializationService = systemInitializationService;
            _localServiceScope = serviceScopeFactory.CreateScope();
            _remoteServerNodeProxy = _localServiceScope.ServiceProvider.GetService<IRemoteServerNodeProxy>();
            _eventRegister = _localServiceScope.ServiceProvider.GetService<IEventRegister>();
            _serverNodeService = _localServiceScope.ServiceProvider.GetService<IServerNodeService>();
            _serviceHealthCheckService = _localServiceScope.ServiceProvider.GetService<IServiceHealthCheckService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (Appsettings.IsAdminConsoleMode)
            {
                await _systemInitializationService.TryInitDefaultEnvironmentAsync();//初始化环境 DEV TEST STAGE PROD
                _systemInitializationService.TryInitJwtSecret();//初始化 jwt secret
                _ = _remoteServerNodeProxy.TestEchoAsync();//开启节点检测
                _ = _serviceHealthCheckService.StartCheckAsync();//开启服务健康检测
                _eventRegister.Register();//注册 eventbus 的回调
            }

            if (Appsettings.Cluster)
            {
                //如果开启集群模式，会自动获取本地的ip注册到节点表，只适合 docker-compose 环境
                var ip = GetIp();
                if (!string.IsNullOrEmpty(ip))
                {
                    var desc = Appsettings.IsAdminConsoleMode ? "控制台节点" : "";
                    _ = _serverNodeService.JoinAsync(ip, 5000, desc);
                    _logger.LogInformation($"AgileConfig node http://{ip}:5000 joined .");
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Appsettings.Cluster)
            {
                var ip = GetIp();
                if (!string.IsNullOrEmpty(ip))
                {
                    await _serverNodeService.DeleteAsync($"http://{ip}:{5000}");
                    _logger.LogInformation($"AgileConfig node http://{ip}:5000 removed .");
                }
            }

            _localServiceScope?.Dispose();
        }

        private string GetIp()
        {
            try
            {
                var myips = IPExt.GetEndpointIp();
                _logger.LogInformation("AgileConfig node's IP " + String.Join(',', myips));

                return myips.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Try get node's IP error .");
            }

            return "";
        }
    }
}
