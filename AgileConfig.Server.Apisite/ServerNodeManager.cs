using AgileConfig.Server.IService;
using AgileHttp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite
{
    public interface IServerNodeManager
    {
        Task TestEchoAsync();
    }
    public class ServerNodeManager: IServerNodeManager
    {
        private IServerNodeService _serverNodeService;
        private ILogger _logger;
        public ServerNodeManager(IServiceCollection sc)
        {
            var serviceProvider = sc.BuildServiceProvider();
            _serverNodeService = serviceProvider.GetService<IServerNodeService>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<ServerNodeManager>();
        }

        public Task TestEchoAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    var nodes = await _serverNodeService.GetAllNodesAsync();
                    nodes.ForEach(n =>
                    {
                        try
                        {
                            using (var resp = n.Address.AsHttp().Send())
                            {
                                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    n.LastEchoTime = DateTime.Now;
                                    n.Status = Data.Entity.NodeStatus.Online;
                                }
                                else
                                {
                                    n.Status = Data.Entity.NodeStatus.Offline;
                                }
                                _serverNodeService.UpdateAsync(n);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogInformation(e, "Try test node {0} echo occur", n.Address);
                        }
                    });

                    await Task.Delay(5000);
                }
            });
        }
    }
}
