using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using System.Runtime.CompilerServices;

namespace AgileConfig.Server.Service.Tests.oracle
{
    [TestClass()]
    public class AppServiceTests
    {
        IFreeSql fsq = null;
        FreeSqlContext freeSqlContext;
        IAppService service = null;

        [TestInitialize]
        public void TestInitialize()
        {
            string conn = "user id=CLINIC;password=CLINIC;data source=192.168.0.91/orcl";
            fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.Oracle, conn)
                          .UseAutoSyncStructure(true)
                          .Build();
            FluentApi.Config(fsq);
            freeSqlContext = new FreeSqlContext(fsq);
            service = new AppService(freeSqlContext);
            fsq.Delete<App>().Where("1=1");

            Console.WriteLine("TestInitialize");
        }
        [TestMethod()]
        public async Task AddAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result = await service.AddAsync(source);
            var app = fsq.Select<App>(new
            {
                Id = id
            }).ToOne();

            Assert.IsTrue(result);
            Assert.IsNotNull(app);

            Assert.AreEqual(source.Id, app.Id);
            Assert.AreEqual(source.Name, app.Name);
            Assert.AreEqual(source.Secret, app.Secret);
           // Assert.AreEqual(source.CreateTime, app.CreateTime);
          //  Assert.AreEqual(source.UpdateTime, app.UpdateTime);
            Assert.AreEqual(source.Enabled, app.Enabled);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var delResult = await service.DeleteAsync(source);
            Assert.IsTrue(delResult);

            var app = fsq.Select<App>(new
            {
                Id = id
            }).ToOne();
            Assert.IsNull(app);

        }

        [TestMethod()]
        public async Task DeleteAsyncTest1()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var delResult = await service.DeleteAsync(id);
            Assert.IsTrue(delResult);

            var app = fsq.Select<App>(new
            {
                Id = id
            }).ToOne();
            Assert.IsNull(app);

        }

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var app = await service.GetAsync(id);
            Assert.IsNotNull(app);

            Assert.AreEqual(source.Id, app.Id);
            Assert.AreEqual(source.Name, app.Name);
            Assert.AreEqual(source.Secret, app.Secret);
          //  Assert.AreEqual(source.CreateTime, app.CreateTime);
          //  Assert.AreEqual(source.UpdateTime, app.UpdateTime);
            Assert.AreEqual(source.Enabled, app.Enabled);
        }

        [TestMethod()]
        public async Task GetAllAppsAsyncTest()
        {
            fsq.Delete<App>().Where("1=1").ExecuteAffrows() ;
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Data.Entity.App
            {
                Id = id1,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);

            var apps = await service.GetAllAppsAsync();
            Assert.IsNotNull(apps);
            Assert.AreEqual(2, apps.Count);


        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            source.Name = "new name";
            source.Secret = "new sec";
            source.CreateTime = DateTime.Now.AddDays(1);
            source.UpdateTime = DateTime.Now.AddDays(1);
            source.Enabled = false;

            var result1 = await service.UpdateAsync(source);
            Assert.IsTrue(result1);

            var app = fsq.Select<App>(new
            {
                Id = id
            }).ToOne();

            Assert.AreEqual(source.Id, app.Id);
            Assert.AreEqual(source.Name, app.Name);
            Assert.AreEqual(source.Secret, app.Secret);
          //  Assert.AreEqual(source.CreateTime, app.CreateTime);
          //  Assert.AreEqual(source.UpdateTime, app.UpdateTime);
            Assert.AreEqual(source.Enabled, app.Enabled);
        }

        [TestMethod()]
        public async Task CountEnabledAppsAsyncTest()
        {
            fsq.Delete<App>().Where("1=1").ExecuteAffrows();
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Data.Entity.App
            {
                Id = id1,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = false
            };
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);

            var count = await service.CountEnabledAppsAsync();
            Assert.AreEqual(1, count);
        }
        [TestMethod()]
        public async Task GetAllInheritancedAppsAsyncTest()
        {
            fsq.Delete<App>().Where("1=1").ExecuteAffrows();
            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.PRIVATE
            };
            var source1 = new Data.Entity.App
            {
                Id = Guid.NewGuid().ToString(),
                Name = "xxx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.PRIVATE
            };
            var source2 = new Data.Entity.App
            {
                Id = Guid.NewGuid().ToString(),
                Name = "xxxx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.Inheritance
            };
            var source3 = new Data.Entity.App
            {
                Id = Guid.NewGuid().ToString(),
                Name = "xxxx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = false,
                Type = AppType.Inheritance
            };
            var result = await service.AddAsync(source);
            await service.AddAsync(source1);
            await service.AddAsync(source2);
            await service.AddAsync(source3);

            Assert.IsTrue(result);

            var apps = await service.GetAllInheritancedAppsAsync();

            Assert.AreEqual(2, apps.Count);
        }
        [TestMethod()]
        public async Task GetInheritancedAppsAsyncTest()
        {
            fsq.Delete<App>().Where("1=1").ExecuteAffrows();
            fsq.Delete<AppInheritanced>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Data.Entity.App
            {
                Id = id,
                Name = "xx",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.PRIVATE
            };
            var source1 = new Data.Entity.App
            {
                Id = Guid.NewGuid().ToString(),
                Name = "xx1",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.Inheritance
            };
            var source2 = new Data.Entity.App
            {
                Id = Guid.NewGuid().ToString(),
                Name = "xx2",
                Secret = "sec",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.Inheritance
            };
            //
            var appInher = new AppInheritanced();
            appInher.Id = Guid.NewGuid().ToString();
            appInher.AppId = source.Id;
            appInher.InheritancedAppId = source1.Id;
            appInher.Sort = 1;
            var appInher1 = new AppInheritanced();
            appInher1.Id = Guid.NewGuid().ToString();
            appInher1.AppId = source.Id;
            appInher1.InheritancedAppId = source2.Id;
            appInher1.Sort = 2;

            var result = await service.AddAsync(source);
            await service.AddAsync(source1);
            await service.AddAsync(source2);
            fsq.Insert<AppInheritanced>(appInher).ExecuteAffrows();
            fsq.Insert<AppInheritanced>(appInher1).ExecuteAffrows();

            Assert.IsTrue(result);

            var apps = await service.GetInheritancedAppsAsync(source.Id);

            Assert.AreEqual(2, apps.Count);
        }
        [TestCleanup]
        public void Clean()
        {
            freeSqlContext.Dispose();
            fsq.Dispose();
        }
    }
}