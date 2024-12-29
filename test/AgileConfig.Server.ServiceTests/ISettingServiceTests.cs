using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.ServiceTests
{
    [TestClass]
    public class ISettingServiceTests
    {
        [TestMethod]
        public void IfEnvEmptySetDefaultTest()
        {
            // Arrange
            string env = "test";
            ISettingService.EnvironmentList = new string[] { "test", "prod" };

            // Act
            ISettingService.IfEnvEmptySetDefault(ref env);

            // Assert
            Assert.AreEqual("test", env);
        }

        [TestMethod]
        public void IfEnvEmptySetDefault_envEmpty_shouldReturn_defaul()
        {
            // Arrange
            string env = "";
            ISettingService.EnvironmentList = new string[] { "test", "prod" };

            // Act
            ISettingService.IfEnvEmptySetDefault(ref env);

            // Assert
            Assert.AreEqual("test", env);
        }

        [TestMethod]
        public void IfEnvEmptySetDefault_envNull_shouldReturn_defaul()
        {
            // Arrange
            string env = null;
            ISettingService.EnvironmentList = new string[] { "test", "prod" };

            // Act
            ISettingService.IfEnvEmptySetDefault(ref env);

            // Assert
            Assert.AreEqual("test", env);
        }
    }
}
