using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Apisite.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using AgileConfig.Server.IService;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using AgileConfig.Server.Service;

namespace AgileConfig.Server.Apisite.Filters.Tests
{
    [TestClass()]
    public class BasicAuthenticationAttributeTests
    {
        [TestMethod()]
        public async Task ValidTest()
        {
            var service = new Mock<IAppService>();
            App app = null;
            service.Setup(s => s.GetAsync("01")).ReturnsAsync(app);
            service.Setup(s => s.GetAsync("02")).ReturnsAsync(new App());
            service.Setup(s => s.GetAsync("03")).ReturnsAsync(new App() {
                Id="03",
                Secret = "1",
            });
            service.Setup(s => s.GetAsync("app01")).ReturnsAsync(new App()
            {
                Id ="app01",
                Secret = "11",
                Enabled = true
            }) ;
            service.Setup(s => s.GetAsync("app02")).ReturnsAsync(new App()
            {
                Id = "app02",
                Secret = null,
                Enabled = true
            });
            service.Setup(s => s.GetAsync("app03")).ReturnsAsync(new App()
            {
                Id = "app03",
                Secret = "",
                Enabled = true
            });
            var http = new DefaultHttpContext();
            var filter = new AppBasicAuthenticationAttribute(new AppBasicAuthService(service.Object));
            var result = await filter.Valid(http.Request);
            Assert.IsFalse(result);
            result = await filter.Valid(http.Request);
            Assert.IsFalse(result);
            result = await filter.Valid(http.Request);
            Assert.IsFalse(result);
            http.Request.Headers["Authorization"] = "Basic YXBwMDE6MTEx=";
            result = await filter.Valid(http.Request);
            Assert.IsFalse(result);
            http.Request.Headers["Authorization"] = "Basic YXBwMDE6MTE=";
            result = await filter.Valid(http.Request);
            Assert.IsTrue(result);

            http.Request.Headers["Authorization"] = "Basic YXBwMDI6";
            result = await filter.Valid(http.Request);
            Assert.IsTrue(result);
            http.Request.Headers["Authorization"] = "Basic YXBwMDM6";
            result = await filter.Valid(http.Request);
            Assert.IsTrue(result);
        }
    }
}