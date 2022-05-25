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
    public class InitService: IHostedService
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IEventRegister _eventRegister;
        private readonly ISettingService _settingService;
        private readonly IServerNodeService _serverNodeService;
        private readonly IServiceHealthCheckService _serviceHealthCheckService;
        private readonly ILogger _logger;
        public InitService(IServiceScopeFactory serviceScopeFactory, ILogger<InitService> logger)
        {
            _logger = logger;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _remoteServerNodeProxy = scope.ServiceProvider.GetService<IRemoteServerNodeProxy>();
                _eventRegister = scope.ServiceProvider.GetService<IEventRegister>();
                _settingService = scope.ServiceProvider.GetService<ISettingService>();
                _serverNodeService = scope.ServiceProvider.GetService<IServerNodeService>();
                _serverNodeService = scope.ServiceProvider.GetService<IServerNodeService>();
                _serviceHealthCheckService = scope.ServiceProvider.GetService<IServiceHealthCheckService>();
            }   
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Appsettings.IsAdminConsoleMode)
            {
                _serverNodeService.InitWatchNodeAsync();
                _settingService.InitDefaultEnvironment();
                _remoteServerNodeProxy.TestEchoAsync();
                _serviceHealthCheckService.StartCheckAsync();
                _eventRegister.Register();
            }

            var myips = IPExt.GetEndpointIp();
            _logger.LogInformation("AgileConfig node's IP " + String.Join(',', myips));

            return  Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
