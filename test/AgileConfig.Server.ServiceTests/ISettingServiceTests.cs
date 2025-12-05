using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.ServiceTests;

[TestClass]
public class ISettingServiceTests
{
    [TestMethod]
    public void IfEnvEmptySetDefaultTest()
    {
        // Arrange
        var env = "test";
        ISettingService.EnvironmentList = new[] { "test", "prod" };

        // Act
        ISettingService.IfEnvEmptySetDefault(ref env);

        // Assert
        Assert.AreEqual("test", env);
    }

    [TestMethod]
    public void IfEnvEmptySetDefault_envEmpty_shouldReturn_defaul()
    {
        // Arrange
        var env = "";
        ISettingService.EnvironmentList = new[] { "test", "prod" };

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
        ISettingService.EnvironmentList = new[] { "test", "prod" };

        // Act
        ISettingService.IfEnvEmptySetDefault(ref env);

        // Assert
        Assert.AreEqual("test", env);
    }
}