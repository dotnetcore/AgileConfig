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
                Secret = "1",
            });
            service.Setup(s => s.GetAsync("app01")).ReturnsAsync(new App()
            {
                Secret = "11",
                Enabled = true
            }) ;

            var http = new DefaultHttpContext();
            var filter = new BasicAuthenticationAttribute(service.Object);
            var result = await filter.Valid(http.Request);
            Assert.IsFalse(result);
            http.Request.Headers["appid"] = "01";
            result = await filter.Valid(http.Request);
            Assert.IsFalse(result);
            http.Request.Headers["appid"] = "02";
            result = await filter.Valid(http.Request);
            Assert.IsTrue(result);
            http.Request.Headers["appid"] = "03";
            http.Request.Headers["Authorization"] = "Basic YXBwMDE6MTE=";
            result = await filter.Valid(http.Request);
            Assert.IsFalse(result);
            http.Request.Headers["appid"] = "app01";
            http.Request.Headers["Authorization"] = "Basic YXBwMDE6MTE=";
            result = await filter.Valid(http.Request);
            Assert.IsTrue(result);
        }
    }
}