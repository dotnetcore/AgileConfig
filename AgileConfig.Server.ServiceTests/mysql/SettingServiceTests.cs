using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using System.Linq;

namespace AgileConfig.Server.Service.Tests.mysql
{
    [TestClass()]
    public class SettingServiceTests
    {
        IFreeSql fsq = null;
        FreeSqlContext freeSqlContext;
        SettingService service = null;

        [TestInitialize]
        public void TestInitialize()
        {
            string conn = "Database=agile_config_test;Data Source=localhost;User Id=root;Password=dev@123;port=3306";
            fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.MySql, conn)
                          .UseAutoSyncStructure(true)
                          .Build();
            freeSqlContext = new FreeSqlContext(fsq);

            service = new SettingService(freeSqlContext);
            fsq.Delete<Setting>().Where("1=1");

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
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var setting = fsq.Select<Setting>(new  {
                Id= id
            }).ToOne();

            Assert.IsNotNull(setting);

            Assert.AreEqual(source.Id, setting.Id);
            Assert.AreEqual(source.Value, setting.Value);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            result = await service.DeleteAsync(source);
            Assert.IsTrue(result);

            var setting = fsq.Select<Setting>(new
            {
                Id = id
            }).ToOne();

            Assert.IsNull(setting);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest1()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            result = await service.DeleteAsync(id);
            Assert.IsTrue(result);

            var setting = fsq.Select<Setting>(new
            {
                Id = id
            }).ToOne();

            Assert.IsNull(setting);
        }

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var setting = await service.GetAsync(id);

            Assert.IsNotNull(setting);

            Assert.AreEqual(source.Id, setting.Id);
            Assert.AreEqual(source.Value, setting.Value);
        }

        [TestMethod()]
        public async Task GetAllSettingsAsyncTest()
        {
            fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Setting();
            source1.Id = id1;
            source1.Value = "123";
            source1.CreateTime = DateTime.Now;
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);

            var settings = await service.GetAllSettingsAsync();

            Assert.IsNotNull(settings);

            Assert.AreEqual(2, settings.Count);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            source.CreateTime = DateTime.Now;
            source.Value = "321";
            var result1 = await service.UpdateAsync(source);
            Assert.IsTrue(result1);

            var setting = await service.GetAsync(id);
            Assert.IsNotNull(setting);

            Assert.AreEqual(source.Id, setting.Id);
            Assert.AreEqual(source.Value, setting.Value);
        }

        [TestMethod()]
        public async Task SetAdminPasswordTest()
        {
            fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            var result = await service.SetAdminPassword("123456");
            Assert.IsTrue(result);
            var list = fsq.Select<Setting>().Where("1=1").ToList();
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);

            var pass = list.FirstOrDefault(s => s.Id == service.AdminPasswordSettingKey);
            Assert.IsNotNull(pass);
            var salt = list.FirstOrDefault(s => s.Id == service.AdminPasswordHashSaltKey);
            Assert.IsNotNull(salt);
        }

        [TestMethod()]
        public async Task HasAdminPasswordTest()
        {
            fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            var result = await service.SetAdminPassword("123456");
            Assert.IsTrue(result);

            var has = await service.HasAdminPassword();
            Assert.IsTrue(has);
            fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            has = await service.HasAdminPassword();
            Assert.IsFalse(has);
        }

        [TestMethod()]
        public async Task ValidateAdminPasswordTest()
        {
            fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            var result = await service.SetAdminPassword("123456");
            Assert.IsTrue(result);

            var v = await service.ValidateAdminPassword("123456");
            Assert.IsTrue(v);
            v = await service.ValidateAdminPassword("1234561");
            Assert.IsFalse(v);
        }
    }
}