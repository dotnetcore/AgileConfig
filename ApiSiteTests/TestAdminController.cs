using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Controllers.api;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ApiSiteTests
{
    [TestClass]
    public class TestAdminController
    {
        [TestMethod]
        public async Task TestLogin()
        {
            var tempData = new Mock<ITempDataDictionary>();
            var settingService = new Mock<ISettingService>();
            var syslogService = new Mock<ISysLogService>();
            var authenticationService = new Mock<IAuthenticationService>();
            authenticationService.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(),It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(),"")));
            var sp = new Mock<IServiceProvider>();
            sp.Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(() => {
                return authenticationService.Object;
            });
            var sl = new ServiceCollection();
            sl.AddMvc();

            var ctrl = new AdminController(settingService.Object, syslogService.Object);
            ctrl.ControllerContext = new ControllerContext();
            ctrl.ControllerContext.HttpContext = new DefaultHttpContext();
            ctrl.ControllerContext.HttpContext.RequestServices = sp.Object;
            ctrl.TempData = tempData.Object;

            var act = await ctrl.Login();
            Assert.IsNotNull(act);
            Assert.IsInstanceOfType(act, typeof(RedirectResult));
            var rd = act as RedirectResult;
            Assert.AreEqual("/", rd.Url);

            authenticationService.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
               .ReturnsAsync(AuthenticateResult.Fail(""));
            settingService.Setup(s => s.HasAdminPassword())
                .ReturnsAsync(false);

            act = await ctrl.Login();
            Assert.IsNotNull(act);
            Assert.IsInstanceOfType(act, typeof(RedirectResult));
            rd = act as RedirectResult;
            Assert.AreEqual("InitPassword", rd.Url);

            settingService.Setup(s => s.HasAdminPassword())
               .ReturnsAsync(true);

            act = await ctrl.Login();
            Assert.IsNotNull(act);
            Assert.IsInstanceOfType(act, typeof(ViewResult));
        }
    }
}
