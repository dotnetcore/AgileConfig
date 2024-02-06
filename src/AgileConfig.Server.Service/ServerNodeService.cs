using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using System;
using System.Linq;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.Service
{
    public class ServerNodeService : IServerNodeService
    {
        private readonly IServerNodeRepository _serverNodeRepository;


        public ServerNodeService(IServerNodeRepository serverNodeRepository)
        {
            _serverNodeRepository = serverNodeRepository;
        }

        public async Task<bool> AddAsync(ServerNode node)
        {
            await _serverNodeRepository.InsertAsync(node);
            return true;
        }

        public async Task<bool> DeleteAsync(ServerNode node)
        {
            var node2 = await _serverNodeRepository.GetAsync(node.Id);
            if (node2 != null)
            {
                await _serverNodeRepository.DeleteAsync(node2);
            }
            return true;
        }

        public async Task<bool> DeleteAsync(string address)
        {
            var node2 = await _serverNodeRepository.GetAsync(address);
            if (node2 != null)
            {
                await _serverNodeRepository.DeleteAsync(node2);
            }

            return true;
        }

        public void Dispose()
        {
            _serverNodeRepository.Dispose();
        }

        public Task<List<ServerNode>> GetAllNodesAsync()
        {
            return _serverNodeRepository.AllAsync();
        }

        public Task<ServerNode> GetAsync(string address)
        {
            return _serverNodeRepository.GetAsync(address);
        }

        public async Task<bool> UpdateAsync(ServerNode node)
        {
            await _serverNodeRepository.UpdateAsync(node);
            return true;
        }


        public async Task<bool> InitWatchNodeAsync()
        {
            var count = await _serverNodeRepository.CountAsync();
            if (count > 0)
            {
                return false;
            }
            var nodes = Global.Config["nodes"];
            var addresses = new List<string>();
            if (!string.IsNullOrEmpty(nodes))
            {
                var arr = nodes.Split(',');
                foreach (var item in arr)
                {
                    var address = "";
                    if (item.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        address = item;
                    }
                    else
                    {
                        address = "http://" + item;
                    }

                    addresses.Add(address);
                }
            }

            var existNodes = await _serverNodeRepository.QueryAsync(x => addresses.Contains(x.Id));
            var newNodes = addresses
                .Where(x => existNodes.All(y => y.Id != x))
                .Select(x => new ServerNode { Id = x, CreateTime = DateTime.Now })
                .ToList();

            if (newNodes.Count == 0)
                return false;

            await _serverNodeRepository.InsertAsync(newNodes);
            return true;
        }

        public async Task<bool> JoinAsync(string ip, int port, string desc)
        {
            var address = $"http://{ip}:{port}";
            var nodes = await _serverNodeRepository.QueryAsync(x => x.Id == address);
            if (nodes.Count > 0)
            {
                nodes.ForEach(n =>
                {
                    n.Id = address;
                    n.Remark = desc;
                    n.Status = NodeStatus.Online;
                });
            }
            else
            {
                await _serverNodeRepository.InsertAsync(new ServerNode
                {
                    Id = address,
                    CreateTime = DateTime.Now,
                    Remark = desc,
                    Status = NodeStatus.Online,
                    LastEchoTime = DateTime.Now
                });
            }

            return true;
        }
    }
}
