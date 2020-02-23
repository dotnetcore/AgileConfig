using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.IService;
using AgileHttp;
using AgileHttp.serialize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite
{
    public interface IRemoteServerNodeManager
    {
        Task TestEchoAsync();

        WebsocketCollectionReport GetClientsReport(string address);
    }

    public class RemoteServerNodeManager : IRemoteServerNodeManager
    {
        internal class ClientsReport
        {
            public WebsocketCollectionReport data { get; set; }
        }
        internal class SerializeProvider : ISerializeProvider
        {
            public T Deserialize<T>(string content)
            {
                return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }

            public string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj);
            }
        }

        private IServerNodeService _serverNodeService;
        private ILogger _logger;
        private ConcurrentDictionary<string, WebsocketCollectionReport> _nodeStatus;

        public RemoteServerNodeManager(IServiceProvider sp)
        {
            _serverNodeService = sp.CreateScope().ServiceProvider.GetService<IServerNodeService>();
            var loggerFactory = sp.GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<RemoteServerNodeManager>();
            _nodeStatus = new ConcurrentDictionary<string, WebsocketCollectionReport>();
        }

        public WebsocketCollectionReport GetClientsReport(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }
            _nodeStatus.TryGetValue(address, out WebsocketCollectionReport report);
            return report;
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
                            using (var resp = (n.Address + "/report/clients").AsHttp().Config(new RequestOptions(new SerializeProvider())).Send())
                            {
                                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    n.LastEchoTime = DateTime.Now;
                                    n.Status = Data.Entity.NodeStatus.Online;
                                    var content = resp.GetResponseContent();
                                    var report = resp.Deserialize<ClientsReport>();
                                    if (report != null)
                                    {
                                        _nodeStatus.AddOrUpdate(n.Address, report.data, (k, r) => report.data);
                                    }
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
