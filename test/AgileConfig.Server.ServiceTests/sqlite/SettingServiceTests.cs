using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using AgileConfig.Server.Service;
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
        public async Task TestInitialize()
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

            ClearData();

            var cache = new Mock<IMemoryCache>();
            IServiceCollection services = new ServiceCollection();
            services.AddScoped(_ => cache.Object);
            services.AddSingleton<IConfiguration>(config);
            services.AddDbConfigInfoFactory();
            services.AddFreeSqlFactory();
            services.AddRepositories();
            services.AddBusinessServices();

            _serviceProvider = services.BuildServiceProvider();
            var systeminitializationService = _serviceProvider.GetService<ISystemInitializationService>();
            systeminitializationService.TryInitDefaultEnvironment();//初始化环境 DEV TEST STAGE PROD
            systeminitializationService.TryInitJwtSecret();//初始化 jwt secret


            _settingService = _serviceProvider.GetService<ISettingService>();

            Console.WriteLine("Run TestInitialize");
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