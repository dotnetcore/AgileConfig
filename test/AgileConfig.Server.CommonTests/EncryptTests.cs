using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Common.Tests;

[TestClass]
public class EncryptTests
{
    private const string Origin = "123456";
    private const string Expected = "E10ADC3949BA59ABBE56E057F20F883E";

    [TestMethod]
    public void Md5Test()
    {
        var result = Encrypt.Md5(Origin);
        Assert.AreEqual(Expected, result);
    }

    [TestMethod]
    public void ParallelCallTest()
    {
        Parallel.For(0, 1000, _ =>
        {
            var result = Encrypt.Md5(Origin);
            Assert.AreEqual(Expected, result);
        });
    }
}