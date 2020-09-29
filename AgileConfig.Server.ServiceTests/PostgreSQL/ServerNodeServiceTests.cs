using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using System.Text;
using AgileConfig.Server.Data.Freesql;
using FreeSql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service.Tests.PostgreSQL
{
    [TestClass()]
    public class ServerNodeServiceTests
    {
        IFreeSql fsq = null;
        FreeSqlContext freeSqlContext;
        ServerNodeService service = null;

        [TestInitialize]
        public void TestInitialize()
        {
            string conn = "Host=127.0.0.1;Database=agile_config;Username=postgres;Password=dev@123";
            fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.PostgreSQL, conn)
                          .UseAutoSyncStructure(true)
                          .Build();
            FluentApi.Config(fsq);
            freeSqlContext = new FreeSqlContext(fsq);

            service = new ServerNodeService(freeSqlContext);
            fsq.Delete<ServerNode>().Where("1=1");

            Console.WriteLine("TestInitialize");
        }



        [TestCleanup]
        public void Clean()
        {
            freeSqlContext.Dispose();
            fsq.Dispose();
        }

        [TestMethod()]
        public async Task AddAsyncTest()
        {
            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();

            var source = new ServerNode();
            source.Address = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var node = fsq.Select<ServerNode>(new {
                Address = "1"
            }).ToOne();
            Assert.IsNotNull(node);

            Assert.AreEqual(source.Address,node.Address);
           // Assert.AreEqual(source.CreateTime, node.CreateTime);
         //   Assert.AreEqual(source.LastEchoTime, node.LastEchoTime);
            Assert.AreEqual(source.Remark, node.Remark);
            Assert.AreEqual(source.Status, node.Status);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();

            var source = new ServerNode();
            source.Address = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var result1 = await service.DeleteAsync(source);
            Assert.IsTrue(result1);

            var node = fsq.Select<ServerNode>(new
            {
                Address = "1"
            }).ToOne();
            Assert.IsNull(node);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest1()
        {
            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();

            var source = new ServerNode();
            source.Address = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var result1 = await service.DeleteAsync(source.Address);
            Assert.IsTrue(result1);

            var node = fsq.Select<ServerNode>(new
            {
                Address = "1"
            }).ToOne();
            Assert.IsNull(node);
        }

        [TestMethod()]
        public async Task GetAllNodesAsyncTest()
        {
            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();

            var source = new ServerNode();
            source.Address = "1";
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

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();

            var source = new ServerNode();
            source.Address = "1";
            source.CreateTime = DateTime.Now;
            source.LastEchoTime = DateTime.Now;
            source.Remark = "2";
            source.Status = NodeStatus.Offline;
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var node = await service.GetAsync(source.Address);
            Assert.IsNotNull(node);

            Assert.AreEqual(source.Address, node.Address);
         //   Assert.AreEqual(source.CreateTime, node.CreateTime);
         //   Assert.AreEqual(source.LastEchoTime, node.LastEchoTime);
            Assert.AreEqual(source.Remark, node.Remark);
            Assert.AreEqual(source.Status, node.Status);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();

            var source = new ServerNode();
            source.Address = "1";
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

            var node = await service.GetAsync(source.Address);
            Assert.IsNotNull(node);

            Assert.AreEqual(source.Address, node.Address);
         //   Assert.AreEqual(source.CreateTime, node.CreateTime);
         //   Assert.AreEqual(source.LastEchoTime, node.LastEchoTime);
            Assert.AreEqual(source.Remark, node.Remark);
            Assert.AreEqual(source.Status, node.Status);
        }
    }
}