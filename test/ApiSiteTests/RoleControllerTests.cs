using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Roles;
using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
public sealed class RoleControllerTests
{
    [TestMethod]
    public async Task ListAndSupportedPermissions_MapApplicationResultsToLegacyEnvelope()
    {
        var service = new Mock<IRoleManagementService>();
        var role = new Role { Id = "role-1", Name = "Role 1", Description = "Description", IsSystem = true };
        service.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<RoleDetails>
        {
            new(role, new[] { "permission-1" })
        });
        service.Setup(x => x.GetSupportedPermissions()).Returns(new[] { "permission-1", "permission-2" });
        var controller = new RoleController(service.Object);

        var listResult = AsJson(await controller.List());
        var roles = Read<IEnumerable<RoleVM>>(listResult, "data").ToList();
        var permissionsResult = AsJson(controller.SupportedPermissions());

        Assert.IsTrue(Read<bool>(listResult, "success"));
        Assert.AreEqual("role-1", roles.Single().Id);
        Assert.IsTrue(roles.Single().IsSystem);
        CollectionAssert.AreEqual(new[] { "permission-1" }, roles.Single().Functions);
        CollectionAssert.AreEqual(
            new[] { "permission-1", "permission-2" },
            Read<IReadOnlyList<string>>(permissionsResult, "data").ToArray());
    }

    [TestMethod]
    public async Task Add_MapsCommandAndApplicationResult()
    {
        var service = new Mock<IRoleManagementService>();
        CreateRoleCommand captured = null;
        service.Setup(x => x.CreateAsync(It.IsAny<CreateRoleCommand>()))
            .Callback<CreateRoleCommand>(command => captured = command)
            .ReturnsAsync(ApplicationResult<Role>.Success(new Role { Id = "role-1" }));
        var controller = new RoleController(service.Object);
        var model = new RoleVM
        {
            Id = "role-1",
            Name = "Role 1",
            Description = "Description",
            Functions = new List<string> { "permission-1" }
        };

        Assert.ThrowsExactly<ArgumentNullException>(() => controller.Add(null).GetAwaiter().GetResult());
        var result = AsJson(await controller.Add(model));

        Assert.IsTrue(Read<bool>(result, "success"));
        Assert.AreEqual(model.Id, captured.Id);
        Assert.AreEqual(model.Name, captured.Name);
        Assert.AreEqual(model.Description, captured.Description);
        CollectionAssert.AreEqual(model.Functions, captured.Functions.ToList());
    }

    [TestMethod]
    public async Task Edit_MapsForbiddenAndOperationFailure()
    {
        var service = new Mock<IRoleManagementService>();
        service.Setup(x => x.UpdateAsync(It.Is<UpdateRoleCommand>(command =>
                command.Id == SystemRoleConstants.SuperAdminId)))
            .ReturnsAsync(ApplicationResult<Role>.Failure(ApplicationError.Forbidden));
        service.Setup(x => x.UpdateAsync(It.Is<UpdateRoleCommand>(command => command.Id == "role-1")))
            .ReturnsAsync(ApplicationResult<Role>.Failure(ApplicationError.OperationFailed));
        var controller = new RoleController(service.Object);

        Assert.ThrowsExactly<ArgumentNullException>(() => controller.Edit(null).GetAwaiter().GetResult());
        var forbidden = AsJson(await controller.Edit(new RoleVM
        {
            Id = SystemRoleConstants.SuperAdminId,
            Name = "Super Administrator"
        }));
        var failed = AsJson(await controller.Edit(new RoleVM { Id = "role-1", Name = "Role 1" }));

        Assert.IsFalse(Read<bool>(forbidden, "success"));
        StringAssert.Contains(Read<string>(forbidden, "message"), "cannot be edited");
        Assert.IsFalse(Read<bool>(failed, "success"));
        Assert.AreEqual(Messages.UpdateRoleFailed, Read<string>(failed, "message"));
    }

    [TestMethod]
    public async Task Delete_MapsForbiddenFailureAndSuccess()
    {
        var service = new Mock<IRoleManagementService>();
        service.Setup(x => x.DeleteAsync(SystemRoleConstants.SuperAdminId))
            .ReturnsAsync(ApplicationResult.Failure(ApplicationError.Forbidden));
        service.Setup(x => x.DeleteAsync("failed"))
            .ReturnsAsync(ApplicationResult.Failure(ApplicationError.OperationFailed));
        service.Setup(x => x.DeleteAsync("deleted"))
            .ReturnsAsync(ApplicationResult.Success());
        var controller = new RoleController(service.Object);

        Assert.ThrowsExactly<ArgumentNullException>(() => controller.Delete(null).GetAwaiter().GetResult());
        var forbidden = AsJson(await controller.Delete(SystemRoleConstants.SuperAdminId));
        var failed = AsJson(await controller.Delete("failed"));
        var deleted = AsJson(await controller.Delete("deleted"));

        StringAssert.Contains(Read<string>(forbidden, "message"), "cannot be deleted");
        Assert.AreEqual(Messages.DeleteRoleFailed, Read<string>(failed, "message"));
        Assert.IsTrue(Read<bool>(deleted, "success"));
        Assert.AreEqual(string.Empty, Read<string>(deleted, "message"));
    }

    private static JsonResult AsJson(IActionResult result)
    {
        Assert.IsInstanceOfType<JsonResult>(result);
        return (JsonResult)result;
    }

    private static T Read<T>(JsonResult result, string propertyName)
    {
        var property = result.Value?.GetType().GetProperty(propertyName);
        Assert.IsNotNull(property);
        return (T)property.GetValue(result.Value);
    }
}
