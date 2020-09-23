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
    }
}
