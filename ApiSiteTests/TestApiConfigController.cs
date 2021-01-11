using AgileConfig.Server.Apisite.Controllers.api;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiSiteTests
{
    [TestClass]
    public class TestApiConfigController
    {
        [TestMethod]
        public async Task TestGet()
        {
            App newApp()
            {
                return new App() {
                    Enabled = true
                };
            }

            var appService = new Mock<IAppService>();
            appService.Setup(s => s.GetAsync("001")).ReturnsAsync(newApp);

            List<Config> newConfigs() {
                var list = new List<Config>();
                list.Add(new Config { 
                    Id = "001",
                    Key ="key1",
                    Value = "val1",
                    Group = ""
                });
                list.Add(new Config
                {
                    Id = "002",
                    Key = "key2",
                    Value = "val2",
                    Group = "group1",
                    AppId = "001",
                    Status = ConfigStatus.Enabled
                });

                return list;
            }
            var configService = new Mock<IConfigService>();
            configService.Setup(s => s.GetPublishedConfigsByAppId("001"))
                .ReturnsAsync(newConfigs);
            configService.Setup(s => s.GetPublishedConfigsByAppIdWithInheritanced("001"))
                .ReturnsAsync(newConfigs);

            var modifyLogService = new Mock<IModifyLogService>();
            var remoteNodeProxy = new Mock<IRemoteServerNodeProxy>();
            var serverNodeService = new Mock<IServerNodeService>();
            var sysLogService = new Mock<ISysLogService>();
            var appBasicAuthService = new Mock<IAppBasicAuthService>();

            var ctrl = new ConfigController(
                configService.Object,
                appService.Object,
                modifyLogService.Object, 
                remoteNodeProxy.Object,
                serverNodeService.Object,
                sysLogService.Object,
                appBasicAuthService.Object);
            var act = await ctrl.GetAppConfig("001");

            Assert.IsNotNull(act);
            Assert.IsNotNull(act.Value);
            Assert.IsInstanceOfType(act.Value, typeof(List<ConfigVM>));
            Assert.AreEqual(2, act.Value.Count);

            App newApp1()
            {
                return new App()
                {
                    Enabled = false
                };
            }
            appService = new Mock<IAppService>();
            appService.Setup(s => s.GetAsync("001")).ReturnsAsync(newApp1);

            ctrl = new ConfigController(
                configService.Object,
                appService.Object,
                modifyLogService.Object,
                remoteNodeProxy.Object,
                serverNodeService.Object,
                sysLogService.Object,
                appBasicAuthService.Object);
            act = await ctrl.GetAppConfig("001");

            Assert.IsNotNull(act);
            Assert.IsNull(act.Value);
            Assert.IsInstanceOfType(act.Result, typeof(NotFoundResult));
        }
    }
}
