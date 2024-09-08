using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Repository.Selector;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Service;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    [TestClass()]
    public class ServerNodeServiceTests : BasicTestService
    {
        IServiceProvider _serviceProvider = null;
        IServiceScope _serviceScope = null;
        IServerNodeService _serverNodeService = null;

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","sqlite" },
                {"db:conn","Data Source=agile_config.db" }
            });
        }
        [TestInitialize]
        public async Task TestInitialize()
        {
            await NewGlobalSp();

            _serviceScope = GlobalServiceProvider.CreateScope();
            _serviceProvider = _serviceScope.ServiceProvider;

            var systeminitializationService = _serviceProvider.GetService<ISystemInitializationService>();
            systeminitializationService.TryInitDefaultEnvironment();//初始化环境 DEV TEST STAGE PROD
            systeminitializationService.TryInitJwtSecret();//初始化 jwt secret

            _serverNodeService = _serviceProvider.GetService<IServerNodeService>();

            Console.WriteLine($"IServerNodeService type is {_serverNodeService.GetType().FullName}");

            Console.WriteLine("Run TestInitialize");
        }

        private async Task NewGlobalSp()
        {
            Console.WriteLine("Try get configration data");
            var dict = await GetConfigurationData();

            foreach (var item in dict)
            {
                Console.WriteLine($"key: {item.Key} value: {item.Value}");
            }

            var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
            Console.WriteLine("Config list");
            foreach (var item in config.AsEnumerable())
            {
                Console.WriteLine($"key: {item.Key} value: {item.Value}");
            }

            var cache = new Mock<IMemoryCache>();
            IServiceCollection services = new ServiceCollection();
            services.AddScoped(_ => cache.Object);
            services.AddSingleton<IConfiguration>(config);
            services.AddDbConfigInfoFactory();
            services.AddFreeSqlFactory();
            services.AddRepositories();
            services.AddBusinessServices();

            this.GlobalServiceProvider = services.BuildServiceProvider();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _serverNodeService.Dispose();
            _serviceScope.Dispose();
        }

        [TestMethod()]
        public async Task AddAsyncTest()
        {
            this.ClearData();

            var source = new ServerNode();
            source.Id = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;

            var result = await _serverNodeService.AddAsync(source);
            Assert.IsTrue(result);

            var node = await _serverNodeService.GetAsync("1");
            Assert.IsNotNull(node);

            Assert.AreEqual(source.Id, node.Id);
            Assert.AreEqual(source.CreateTime.ToString("yyyyMMddHHmmss"), node.CreateTime.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.LastEchoTime.Value.ToString("yyyyMMddHHmmss"), node.LastEchoTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.Remark, node.Remark);
            Assert.AreEqual(source.Status, node.Status);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            this.ClearData();

            var source = new ServerNode();
            source.Id = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;

            var result = await _serverNodeService.AddAsync(source);
            Assert.IsTrue(result);

            var result1 = await _serverNodeService.DeleteAsync(source);
            Assert.IsTrue(result1);

            var node = await _serverNodeService.GetAsync("1");

            Assert.IsNull(node);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest1()
        {
            this.ClearData();

            var source = new ServerNode();
            source.Id = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;

            var result = await _serverNodeService.AddAsync(source);
            Assert.IsTrue(result);

            var result1 = await _serverNodeService.DeleteAsync(source.Id);
            Assert.IsTrue(result1);

            var node = await _serverNodeService.GetAsync("1");

            Assert.IsNull(node);
        }

        [TestMethod()]
        public async Task GetAllNodesAsyncTest()
        {
            this.ClearData();

            var source = new ServerNode();
            source.Id = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;

            var result = await _serverNodeService.AddAsync(source);
            Assert.IsTrue(result);

            var nodes = await _serverNodeService.GetAllNodesAsync();
            Assert.IsNotNull(nodes);

            Assert.AreEqual(1, nodes.Count);
        }

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            this.ClearData();

            var source = new ServerNode();
            source.Id = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;
            var result = await _serverNodeService.AddAsync(source);
            Assert.IsTrue(result);

            var node = await _serverNodeService.GetAsync(source.Id);
            Assert.IsNotNull(node);

            Assert.AreEqual(source.Id, node.Id);
            Assert.AreEqual(source.CreateTime.ToString("yyyyMMddHHmmss"), node.CreateTime.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.LastEchoTime.Value.ToString("yyyyMMddHHmmss"), node.LastEchoTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.Remark, node.Remark);
            Assert.AreEqual(source.Status, node.Status);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            this.ClearData();

            var source = new ServerNode();
            source.Id = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;
            var result = await _serverNodeService.AddAsync(source);
            Assert.IsTrue(result);

            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "3";
            source.Status = NodeStatus.Online;
            var result1 = await _serverNodeService.UpdateAsync(source);
            Assert.IsTrue(result);

            var node = await _serverNodeService.GetAsync(source.Id);
            Assert.IsNotNull(node);

            Assert.AreEqual(source.Id, node.Id);
            Assert.AreEqual(source.CreateTime.ToString("yyyyMMddHHmmss"), node.CreateTime.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.LastEchoTime.Value.ToString("yyyyMMddHHmmss"), node.LastEchoTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.Remark, node.Remark);
            Assert.AreEqual(source.Status, node.Status);
        }
    }
}