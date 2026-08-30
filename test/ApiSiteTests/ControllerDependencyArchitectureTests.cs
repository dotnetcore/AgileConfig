using System.Linq;
using AgileConfig.Server.Apisite;
using AgileConfig.Server.Apisite.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiSiteTests;

[TestClass]
public sealed class ControllerDependencyArchitectureTests
{
    [TestMethod]
    public void Controllers_DoNotDependOnOtherControllers()
    {
        var controllerType = typeof(ControllerBase);
        var invalidDependencies = typeof(Startup).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && controllerType.IsAssignableFrom(type))
            .SelectMany(type => type.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters()
                    .Where(parameter => controllerType.IsAssignableFrom(parameter.ParameterType))
                    .Select(parameter => $"{type.FullName} -> {parameter.ParameterType.FullName}")))
            .ToList();

        Assert.IsEmpty(invalidDependencies,
            $"Controllers must call application services instead of other controllers: {string.Join(", ", invalidDependencies)}");
    }

    [TestMethod]
    public void V2Controllers_UseApplicationServicesForUseCases()
    {
        var invalidDependencies = typeof(Startup).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract &&
                           typeof(ControllerBase).IsAssignableFrom(type) &&
                           type.Namespace == "AgileConfig.Server.Apisite.Controllers.api.v2")
            .SelectMany(type => type.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters()
                    .Where(parameter =>
                        parameter.ParameterType.Namespace != "AgileConfig.Server.Application" &&
                        parameter.ParameterType.Namespace != "AgileConfig.Server.Application.Configurations" &&
                        parameter.ParameterType.Namespace != "AgileConfig.Server.Application.Releases" &&
                        parameter.ParameterType.Namespace != "AgileConfig.Server.Apisite.Metrics")
                    .Select(parameter => $"{type.FullName} -> {parameter.ParameterType.FullName}")))
            .ToList();

        Assert.IsEmpty(invalidDependencies,
            $"V2 controllers must access use cases through the Application layer: {string.Join(", ", invalidDependencies)}");
    }

    [TestMethod]
    public void MigratedIdentityControllers_UseOnlyApplicationServices()
    {
        var migratedControllers = new[] { typeof(UserController), typeof(RoleController) };
        var invalidDependencies = migratedControllers
            .SelectMany(type => type.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters()
                    .Where(parameter => parameter.ParameterType.Namespace == null ||
                                        !parameter.ParameterType.Namespace.StartsWith(
                                            "AgileConfig.Server.Application"))
                    .Select(parameter => $"{type.FullName} -> {parameter.ParameterType.FullName}")))
            .ToList();

        Assert.IsEmpty(invalidDependencies,
            $"Migrated identity controllers must use Application services: {string.Join(", ", invalidDependencies)}");
    }
}
