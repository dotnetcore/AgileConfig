using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.EntityFrameworkCore;
using System;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Service
{
    public class ServerNodeService : IServerNodeService
    {
        private FreeSqlContext _dbContext;

        public ServerNodeService(FreeSqlContext context)
        {
            _dbContext = context;
        }

        public async Task<bool> AddAsync(ServerNode node)
        {
            await _dbContext.ServerNodes.AddAsync(node);
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        public async Task<bool> DeleteAsync(ServerNode node)
        {
            node = await _dbContext.ServerNodes.Where(n => n.Address == node.Address).ToOneAsync();
            if (node != null)
            {
                _dbContext.ServerNodes.Remove(node);
            }
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        public async Task<bool> DeleteAsync(string address)
        {
            var node = await _dbContext.ServerNodes.Where(n => n.Address == address).ToOneAsync();
            if (node != null)
            {
                _dbContext.ServerNodes.Remove(node);
            }
            int x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public async Task<List<ServerNode>> GetAllNodesAsync()
        {
            return await _dbContext.ServerNodes.Where(n => 1 == 1).ToListAsync();
        }

        public async Task<ServerNode> GetAsync(string address)
        {
           return await _dbContext.ServerNodes.Where(n => n.Address == address).ToOneAsync();
        }

        public async Task<bool> UpdateAsync(ServerNode node)
        {
            _dbContext.Update(node);
            var x = await _dbContext.SaveChangesAsync();
            var result = x > 0;

            return result;
        }

        
        public async Task<bool> InitWatchNodeAsync()
        {
            var count = await _dbContext.ServerNodes.Select.CountAsync();
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
            
            foreach (var address in addresses)
            {
                var node = await _dbContext.ServerNodes.Where(n => n.Address == address).ToOneAsync();
                if (node == null)
                {
                    node = new ServerNode()
                    {
                        Address = address,
                        CreateTime = DateTime.Now,
                    };
                    await _dbContext.ServerNodes.AddAsync(node);
                }
            }

            var result = 0;
            if (addresses.Count > 0)
            {
                result = await _dbContext.SaveChangesAsync();
            }

            return result > 0;
        }

        public async Task<bool> JoinAsync(string ip, int port, string desc)
        {
            var address = $"http://{ip}:{port}";
            var nodes = await _dbContext.ServerNodes.Where(x => x.Address == address).ToListAsync();
            if (nodes.Count > 0)
            {
                nodes.ForEach(n => {
                    n.Address = address;
                    n.Remark = desc;
                    n.Status = NodeStatus.Online;
                });
            }
            else
            {
                await _dbContext.ServerNodes.AddAsync(new ServerNode { 
                    Address = address,
                    CreateTime = DateTime.Now,
                    Remark = desc,
                    Status = NodeStatus.Online,
                    LastEchoTime = DateTime.Now
                });
            }

            var effRows = await _dbContext.SaveChangesAsync();
            return effRows > 0;
        }
    }
}
