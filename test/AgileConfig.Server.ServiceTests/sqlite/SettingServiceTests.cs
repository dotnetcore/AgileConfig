using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    [TestClass()]
    public class SettingServiceTests : BasicTestService
    {
        ISettingService _settingService = null;
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
        public void TestInitialize()
        {
            _settingService = _serviceProvider.GetService<ISettingService>();
            this.ClearData();
        }


        [TestMethod()]
        public async Task AddAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await _settingService.AddAsync(source);
            Assert.IsTrue(result);

            var setting = await _settingService.GetAsync(source.Id);

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
            var result = await _settingService.AddAsync(source);
            Assert.IsTrue(result);

            result = await _settingService.DeleteAsync(source);
            Assert.IsTrue(result);

            var setting = await _settingService.GetAsync(source.Id);

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
            var result = await _settingService.AddAsync(source);
            Assert.IsTrue(result);

            result = await _settingService.DeleteAsync(id);
            Assert.IsTrue(result);

            var setting = await _settingService.GetAsync(source.Id);

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
            var result = await _settingService.AddAsync(source);
            Assert.IsTrue(result);

            var setting = await _settingService.GetAsync(id);

            Assert.IsNotNull(setting);

            Assert.AreEqual(source.Id, setting.Id);
            Assert.AreEqual(source.Value, setting.Value);
        }

        [TestMethod()]
        public async Task GetAllSettingsAsyncTest()
        {
            this.ClearData();

             var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await _settingService.AddAsync(source);
            Assert.IsTrue(result);
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Setting();
            source1.Id = id1;
            source1.Value = "123";
            source1.CreateTime = DateTime.Now;
            var result1 = await _settingService.AddAsync(source1);
            Assert.IsTrue(result1);

            var settings = await _settingService.GetAllSettingsAsync();

            Assert.IsNotNull(settings);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Setting();
            source.Id = id;
            source.Value = "123";
            source.CreateTime = DateTime.Now;
            var result = await _settingService.AddAsync(source);
            Assert.IsTrue(result);

            source.CreateTime = DateTime.Now;
            source.Value = "321";
            var result1 = await _settingService.UpdateAsync(source);
            Assert.IsTrue(result1);

            var setting = await _settingService.GetAsync(id);
            Assert.IsNotNull(setting);

            Assert.AreEqual(source.Id, setting.Id);
            Assert.AreEqual(source.Value, setting.Value);
        }

    }
}