using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using AgileConfig.Server.Data.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.EntityFrameworkCore;
using System;

namespace AgileConfig.Server.Service
{
    public class ServerNodeService : IServerNodeService
    {
        private AgileConfigDbContext _dbContext;
        private ISysLogService _sysLogService;

        public ServerNodeService(ISqlContext context, ISysLogService sysLogService)
        {
            _dbContext = context as AgileConfigDbContext;
            _sysLogService = sysLogService;
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
            node = _dbContext.ServerNodes.Find(node.Address);
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
            var node = _dbContext.ServerNodes.Find(address);
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
            return await _dbContext.ServerNodes.ToListAsync();
        }

        public async Task<ServerNode> GetAsync(string address)
        {
            return await _dbContext.ServerNodes.FindAsync(address);
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
