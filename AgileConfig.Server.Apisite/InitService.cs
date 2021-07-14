using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Hosting;

namespace AgileConfig.Server.Apisite
{
    public class InitService: IHostedService
    {
        private IRemoteServerNodeProxy _remoteServerNodeProxy;
        private IEventRegister _eventRegister;
        public InitService(IRemoteServerNodeProxy proxy, IEventRegister eventRegister)
        {
            _remoteServerNodeProxy = proxy;
            _eventRegister = eventRegister;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _remoteServerNodeProxy.TestEchoAsync();
            _eventRegister.Init();
            return  Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
