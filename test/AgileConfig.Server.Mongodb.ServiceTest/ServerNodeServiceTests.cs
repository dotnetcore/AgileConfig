using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Mongodb.ServiceTest;

public class ServerNodeServiceTests : DatabaseFixture
{
    private IServerNodeService service;

    [SetUp]
    public async Task TestInitialize()
    {
        service = new ServerNodeService(ServerNodeRepository);

        await ServerNodeRepository.DeleteAsync(x => true);

        Console.WriteLine("TestInitialize");
    }

    [Test]
    public async Task AddAsyncTest()
    {
        await ServerNodeRepository.DeleteAsync(x => true);

        var source = new ServerNode();
        source.Id = "1";
        source.CreateTime = DateTime.Now;
        source.LastEchoTime = DateTime.Now;
        source.Remark = "2";
        source.Status = NodeStatus.Offline;

        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var node = await ServerNodeRepository.SearchFor(x => x.Id == "1").FirstOrDefaultAsync();
        Assert.IsNotNull(node);

        Assert.AreEqual(source.Id, node.Id);
        // Assert.AreEqual(source.CreateTime, node.CreateTime);
        //   Assert.AreEqual(source.LastEchoTime, node.LastEchoTime);
        Assert.AreEqual(source.Remark, node.Remark);
        Assert.AreEqual(source.Status, node.Status);
    }

    [Test]
    public async Task DeleteAsyncTest()
    {
        await ServerNodeRepository.DeleteAsync(x => true);

        var source = new ServerNode();
        source.Id = "1";
        source.CreateTime = DateTime.Now;
        source.LastEchoTime = DateTime.Now;
        source.Remark = "2";
        source.Status = NodeStatus.Offline;

        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var result1 = await service.DeleteAsync(source);
        Assert.IsTrue(result1);

        var node = await ServerNodeRepository.SearchFor(x => x.Id == "1").FirstOrDefaultAsync();
        Assert.IsNull(node);
    }

    [Test]
    public async Task DeleteAsyncTest1()
    {
        await ServerNodeRepository.DeleteAsync(x => true);

        var source = new ServerNode();
        source.Id = "1";
        source.CreateTime = DateTime.Now;
        source.LastEchoTime = DateTime.Now;
        source.Remark = "2";
        source.Status = NodeStatus.Offline;

        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var result1 = await service.DeleteAsync(source.Id);
        Assert.IsTrue(result1);

        var node = await ServerNodeRepository.SearchFor(x => x.Id == "1").FirstOrDefaultAsync();
        Assert.IsNull(node);
    }

    [Test]
    public async Task GetAllNodesAsyncTest()
    {
        await ServerNodeRepository.DeleteAsync(x => true);

        var source = new ServerNode();
        source.Id = "1";
        source.CreateTime = DateTime.Now;
        source.LastEchoTime = DateTime.Now;
        source.Remark = "2";
        source.Status = NodeStatus.Offline;

        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var nodes = await service.GetAllNodesAsync();
        Assert.IsNotNull(nodes);

        Assert.AreEqual(1, nodes.Count);
    }

    [Test]
    public async Task GetAsyncTest()
    {
        await ServerNodeRepository.DeleteAsync(x => true);

        var source = new ServerNode();
        source.Id = "1";
        source.CreateTime = DateTime.Now;
        source.LastEchoTime = DateTime.Now;
        source.Remark = "2";
        source.Status = NodeStatus.Offline;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var node = await service.GetAsync(source.Id);
        Assert.IsNotNull(node);

        Assert.AreEqual(source.Id, node.Id);
        //   Assert.AreEqual(source.CreateTime, node.CreateTime);
        //   Assert.AreEqual(source.LastEchoTime, node.LastEchoTime);
        Assert.AreEqual(source.Remark, node.Remark);
        Assert.AreEqual(source.Status, node.Status);
    }

    [Test]
    public async Task UpdateAsyncTest()
    {
        await ServerNodeRepository.DeleteAsync(x => true);

        var source = new ServerNode();
        source.Id = "1";
        source.CreateTime = DateTime.Now;
        source.LastEchoTime = DateTime.Now;
        source.Remark = "2";
        source.Status = NodeStatus.Offline;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        source.CreateTime = DateTime.Now;
        source.LastEchoTime = DateTime.Now;
        source.Remark = "3";
        source.Status = NodeStatus.Online;
        var result1 = await service.UpdateAsync(source);
        Assert.IsTrue(result);

        var node = await service.GetAsync(source.Id);
        Assert.IsNotNull(node);

        Assert.AreEqual(source.Id, node.Id);
        //   Assert.AreEqual(source.CreateTime, node.CreateTime);
        //   Assert.AreEqual(source.LastEchoTime, node.LastEchoTime);
        Assert.AreEqual(source.Remark, node.Remark);
        Assert.AreEqual(source.Status, node.Status);
    }
}