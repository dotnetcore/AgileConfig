using System.Linq;
using AgileConfig.Server.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.ApplicationTests;

[TestClass]
public sealed class ApplicationLayerDependencyTests
{
    [TestMethod]
    public void ApplicationAssembly_DoesNotReferenceHttpOrApisiteAssemblies()
    {
        var references = typeof(ApplicationResult).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToList();

        Assert.IsFalse(references.Any(x => x.StartsWith("Microsoft.AspNetCore")));
        Assert.DoesNotContain(references, "AgileConfig.Server.Apisite");
    }
}
