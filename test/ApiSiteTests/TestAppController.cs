using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
public class TestAppController
{
    [TestMethod]
    public async Task TestAdd()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);

        var appService = new Mock<IAppService>();
        var logService = new Mock<ISysLogService>();
        var userService = new Mock<IUserService>();
        var permissionService = new Mock<IPermissionService>();
        var eventBus = new Mock<ITinyEventBus>();

        var ctl = new AppController(appService.Object, permissionService.Object, userService.Object, eventBus.Object);

        ctl.ControllerContext.HttpContext = new DefaultHttpContext();

        Assert.ThrowsException<ArgumentNullException>(() => { ctl.Add(null).GetAwaiter().GetResult(); });

        appService.Setup(s => s.GetAsync("01")).ReturnsAsync(new App());
        var result = await ctl.Add(new AppVM
        {
            Id = "01"
        });
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        var jr = result as JsonResult;
        Assert.IsNotNull(jr.Value);
        Console.WriteLine(jr.Value.ToString());
        //Assert.IsTrue(jr.Value.ToString().Contains("Ӧ��Id�Ѵ��ڣ�����������"));
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
        Console.WriteLine(jr.Value.ToString());
        Assert.IsTrue(jr.Value.ToString().Contains("success = False"));

        appService.Setup(s => s.AddAsync(It.IsAny<App>())).ReturnsAsync(true);
        appService.Setup(s => s.AddAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>())).ReturnsAsync(true);
        Console.WriteLine(CultureInfo.CurrentUICulture);
        result = await ctl.Add(new AppVM
        {
            Id = "02"
        });
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        jr = result as JsonResult;
        Assert.IsNotNull(jr.Value);
        Console.WriteLine(jr.Value.ToString());
        Assert.IsTrue(jr.Value.ToString().Contains("success = True"));
    }
}