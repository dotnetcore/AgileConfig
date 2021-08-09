using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Controllers.api;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiSiteTests
{
    [TestClass]
    public class TestAppController
    {
        [TestMethod]
        public async Task TestAdd()
        {
            var appService = new Mock<IAppService>();
            var logService = new Mock<ISysLogService>();
            var userService = new Mock<IUserService>();
            var permissionService = new Mock<IPremissionService>();

            var ctl = new AgileConfig.Server.Apisite.Controllers.AppController(appService.Object, permissionService.Object, userService.Object);
            Assert.ThrowsException<ArgumentNullException>( () => {
                ctl.Add(null).GetAwaiter().GetResult();
            });

            appService.Setup(s => s.GetAsync("01")).ReturnsAsync(new App());
            var result = await ctl.Add(new AppVM
            {
                Id = "01"
            });
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var jr = result as JsonResult;
            Assert.IsNotNull(jr.Value);
            Assert.IsTrue(jr.Value.ToString().Contains("Ӧ��Id�Ѵ��ڣ�����������"));
            App nullApp = null;

            appService.Setup(s => s.GetAsync("02")).ReturnsAsync(nullApp);
            appService.Setup(s => s.AddAsync(It.IsAny<App>())).ReturnsAsync(false);
            result = await ctl.Add(new AppVM
            {
                Id = "02"
            });
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            jr = result as JsonResult;
            Assert.IsNotNull(jr.Value);
            Assert.IsTrue(jr.Value.ToString().Contains("�½�Ӧ��ʧ�ܣ���鿴������־"));

            appService.Setup(s => s.AddAsync(It.IsAny<App>())).ReturnsAsync(true);
            appService.Setup(s => s.AddAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>())).ReturnsAsync(true);

            result = await ctl.Add(new AppVM
            {
                Id = "02"
            });
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            jr = result as JsonResult;
            Assert.IsNotNull(jr.Value);
            Assert.IsFalse(jr.Value.ToString().Contains("�½�Ӧ��ʧ�ܣ���鿴������־"));
        }
    }
}
