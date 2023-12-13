using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Mongodb.Service;

public class ServerNodeService(IRepository<ServerNode> repository) : IServerNodeService
{
    public async Task<bool> AddAsync(ServerNode node)
    {
        await repository.InsertAsync(node);
        return true;
    }

    public async Task<bool> DeleteAsync(ServerNode node)
    {
        var result = await repository.DeleteAsync(x => x.Address == node.Address);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteAsync(string address)
    {
        var result = await repository.DeleteAsync(x => x.Address == address);
        return result.DeletedCount > 0;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<List<ServerNode>> GetAllNodesAsync()
    {
        return repository.SearchFor(x => true).ToListAsync();
    }

    public Task<ServerNode> GetAsync(string address)
    {
        return repository.SearchFor(n => n.Address == address).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(ServerNode node)
    {
        var result = await repository.UpdateAsync(node);
        return result.ModifiedCount > 0;
    }


    public async Task<bool> InitWatchNodeAsync()
    {
        var count = await repository.SearchFor(x => true).CountAsync();
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

        var existNodes = await repository.SearchFor(x => addresses.Contains(x.Address)).ToListAsync();
        var newNodes = addresses
            .Where(x => existNodes.All(y => y.Address != x))
            .Select(x => new ServerNode { Address = x, CreateTime = DateTime.Now })
            .ToList();
        await repository.InsertAsync(newNodes);

        return true;
    }

    public async Task<bool> JoinAsync(string ip, int port, string desc)
    {
        var address = $"http://{ip}:{port}";
        var nodes = await repository.SearchFor(x => x.Address == address).ToListAsync();
        if (nodes.Count > 0)
        {
            nodes.ForEach(n =>
            {
                n.Address = address;
                n.Remark = desc;
                n.Status = NodeStatus.Online;
            });
        }
        else
        {
            await repository.InsertAsync(new ServerNode
            {
                Address = address,
                CreateTime = DateTime.Now,
                Remark = desc,
                Status = NodeStatus.Online,
                LastEchoTime = DateTime.Now
            });
        }
        return true;
    }
}