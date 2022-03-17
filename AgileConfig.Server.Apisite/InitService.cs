using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AgileConfig.Server.Apisite
{
    public class InitService: IHostedService
    {
        private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
        private readonly IEventRegister _eventRegister;
        private readonly ISettingService _settingService;
        private readonly IServerNodeService _serverNodeService;
        private readonly IServiceHealthCheckService _serviceHealthCheckService;
        public InitService(IServiceScopeFactory serviceScopeFactory)
        {
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

            return  Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
