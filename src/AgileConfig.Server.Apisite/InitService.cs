using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite
{
    public class InitService : IHostedService
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IEventHandlerRegister _eventRegister;
        private readonly IServerNodeService _serverNodeService;
        private readonly IServiceHealthCheckService _serviceHealthCheckService;
        private readonly ISystemInitializationService _systemInitializationService;
        private readonly IMeterService _meterService;
        private readonly ILogger _logger;
        private readonly IServiceScope _localServiceScope;
        public InitService(IServiceScopeFactory serviceScopeFactory,
            ISystemInitializationService systemInitializationService,
            IMeterService meterService,
            ILogger<InitService> logger)
        {
            _logger = logger;
            _systemInitializationService = systemInitializationService;
            _meterService = meterService;
            _localServiceScope = serviceScopeFactory.CreateScope();
            _remoteServerNodeProxy = _localServiceScope.ServiceProvider.GetService<IRemoteServerNodeProxy>();
            _eventRegister = _localServiceScope.ServiceProvider.GetService<IEventHandlerRegister>();
            _serverNodeService = _localServiceScope.ServiceProvider.GetService<IServerNodeService>();
            _serviceHealthCheckService = _localServiceScope.ServiceProvider.GetService<IServiceHealthCheckService>();

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _systemInitializationService.TryInitDefaultEnvironment();//init DEV TEST STAGE PROD

            if (Appsettings.IsAdminConsoleMode)
            {
                _systemInitializationService.TryInitJwtSecret();//初始化 jwt secret
                _systemInitializationService.TryInitSaPassword(); // init super admin password
                _systemInitializationService.TryInitDefaultApp();
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
                    var desc = Appsettings.IsAdminConsoleMode ? "Console node" : "";
                    await _serverNodeService.JoinAsync(ip, 5000, desc);
                    _logger.LogInformation($"AgileConfig node http://{ip}:5000 joined .");
                }
            }

            if (!string.IsNullOrEmpty(Appsettings.OtlpMetricsEndpoint))
            {
                _meterService.Start();
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
                var myips = IpExt.GetEndpointIp();
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
