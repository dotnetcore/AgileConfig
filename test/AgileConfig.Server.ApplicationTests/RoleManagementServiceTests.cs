using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Roles;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ApplicationTests;

[TestClass]
public sealed class RoleManagementServiceTests
{
    [TestMethod]
    public async Task GetAllAsync_FiltersSuperAdminSortsAndLoadsFunctions()
    {
        var roleService = new Mock<IRoleService>();
        var superAdmin = new Role
        {
            Id = SystemRoleConstants.SuperAdminId,
            Name = "Super Administrator",
            IsSystem = true
        };
        var operatorRole = new Role
        {
            Id = SystemRoleConstants.OperatorId,
            Name = "Operator",
            IsSystem = true
        };
        var adminRole = new Role
        {
            Id = SystemRoleConstants.AdminId,
            Name = "Administrator",
            IsSystem = true
        };
        var zetaRole = new Role { Id = "zeta", Name = "Zeta", IsSystem = false };
        var alphaRole = new Role { Id = "alpha", Name = "Alpha", IsSystem = false };

        roleService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Role> { zetaRole, superAdmin, operatorRole, alphaRole, adminRole });
        roleService.Setup(x => x.GetFunctionsAsync(adminRole.Id))
            .ReturnsAsync(new List<string> { Functions.Role_Read });
        roleService.Setup(x => x.GetFunctionsAsync(operatorRole.Id))
            .ReturnsAsync(new List<string> { Functions.App_Read, Functions.Config_Read });
        roleService.Setup(x => x.GetFunctionsAsync(alphaRole.Id))
            .ReturnsAsync(new List<string>());
        roleService.Setup(x => x.GetFunctionsAsync(zetaRole.Id))
            .ReturnsAsync(new List<string> { Functions.Log_Read });

        var result = await CreateService(roleService).GetAllAsync();

        Assert.AreEqual(4, result.Count);
        Assert.AreSame(adminRole, result[0].Role);
        Assert.AreSame(operatorRole, result[1].Role);
        Assert.AreSame(alphaRole, result[2].Role);
        Assert.AreSame(zetaRole, result[3].Role);
        CollectionAssert.AreEqual(new[] { Functions.Role_Read }, result[0].Functions.ToArray());
        CollectionAssert.AreEqual(
            new[] { Functions.App_Read, Functions.Config_Read },
            result[1].Functions.ToArray());
        Assert.IsFalse(result.Any(x => x.Role.Id == SystemRoleConstants.SuperAdminId));
        roleService.Verify(x => x.GetFunctionsAsync(SystemRoleConstants.SuperAdminId), Times.Never);
        roleService.Verify(x => x.GetFunctionsAsync(It.IsAny<string>()), Times.Exactly(4));
    }

    [TestMethod]
    public async Task CreateAsync_MapsRoleAndNormalizesFunctions()
    {
        var roleService = new Mock<IRoleService>();
        Role createdRole = null;
        List<string> createdFunctions = null;
        roleService.Setup(x => x.GetAsync("custom-role"))
            .ReturnsAsync((Role)null);
        roleService.Setup(x => x.CreateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()))
            .Callback<Role, IEnumerable<string>>((role, functions) =>
            {
                createdRole = role;
                createdFunctions = functions.ToList();
            })
            .ReturnsAsync((Role role, IEnumerable<string> _) => role);

        var result = await CreateService(roleService).CreateAsync(new CreateRoleCommand(
            "custom-role",
            "Custom role",
            null,
            new[] { Functions.Role_Read, " ", Functions.Role_Read, null, Functions.App_Read }));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(createdRole, result.Value);
        Assert.AreEqual("custom-role", createdRole.Id);
        Assert.AreEqual("Custom role", createdRole.Name);
        Assert.AreEqual(string.Empty, createdRole.Description);
        Assert.IsFalse(createdRole.IsSystem);
        CollectionAssert.AreEqual(
            new[] { Functions.Role_Read, Functions.App_Read },
            createdFunctions);
        roleService.Verify(x => x.CreateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_WhenIdentifierExistsReturnsConflictWithoutWriting()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync("existing"))
            .ReturnsAsync(new Role { Id = "existing" });

        var result = await CreateService(roleService).CreateAsync(new CreateRoleCommand(
            "existing", "Duplicate", "description", Array.Empty<string>()));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Conflict, result.Error);
        roleService.Verify(x => x.CreateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenRoleDoesNotExistReturnsNotFoundWithoutWriting()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync("missing"))
            .ReturnsAsync((Role)null);

        var result = await CreateService(roleService).UpdateAsync(new UpdateRoleCommand(
            "missing", "Updated", null, Array.Empty<string>()));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        roleService.Verify(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenRoleIsSuperAdminReturnsForbiddenWithoutWriting()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync(SystemRoleConstants.SuperAdminId))
            .ReturnsAsync(new Role
            {
                Id = SystemRoleConstants.SuperAdminId,
                Name = "Super Administrator",
                IsSystem = true
            });

        var result = await CreateService(roleService).UpdateAsync(new UpdateRoleCommand(
            SystemRoleConstants.SuperAdminId,
            "Changed",
            "Changed description",
            new[] { Functions.Role_Edit }));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Forbidden, result.Error);
        roleService.Verify(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_PreservesSystemFlagAndMapsNormalizedFunctions()
    {
        var roleService = new Mock<IRoleService>();
        var existingRole = new Role
        {
            Id = "system-role",
            Name = "Old name",
            Description = "Old description",
            IsSystem = true
        };
        Role updatedRole = null;
        List<string> updatedFunctions = null;
        roleService.Setup(x => x.GetAsync(existingRole.Id)).ReturnsAsync(existingRole);
        roleService.Setup(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()))
            .Callback<Role, IEnumerable<string>>((role, functions) =>
            {
                updatedRole = role;
                updatedFunctions = functions.ToList();
            })
            .ReturnsAsync(true);

        var result = await CreateService(roleService).UpdateAsync(new UpdateRoleCommand(
            existingRole.Id,
            "New name",
            null,
            new[] { Functions.Config_Read, "", Functions.Config_Read }));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(updatedRole, result.Value);
        Assert.AreEqual(existingRole.Id, updatedRole.Id);
        Assert.AreEqual("New name", updatedRole.Name);
        Assert.AreEqual(string.Empty, updatedRole.Description);
        Assert.IsTrue(updatedRole.IsSystem);
        CollectionAssert.AreEqual(new[] { Functions.Config_Read }, updatedFunctions);
        roleService.Verify(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenPersistenceFailsReturnsOperationFailed()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync("role-1"))
            .ReturnsAsync(new Role { Id = "role-1", IsSystem = false });
        roleService.Setup(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(false);

        var result = await CreateService(roleService).UpdateAsync(new UpdateRoleCommand(
            "role-1", "Updated", "description", new[] { Functions.Role_Read }));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.OperationFailed, result.Error);
        roleService.Verify(x => x.UpdateAsync(It.IsAny<Role>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenRoleDoesNotExistReturnsNotFoundWithoutDeleting()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync("missing"))
            .ReturnsAsync((Role)null);

        var result = await CreateService(roleService).DeleteAsync("missing");

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        roleService.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenRoleIsSystemReturnsForbiddenWithoutDeleting()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync(SystemRoleConstants.AdminId))
            .ReturnsAsync(new Role
            {
                Id = SystemRoleConstants.AdminId,
                Name = "Administrator",
                IsSystem = true
            });

        var result = await CreateService(roleService).DeleteAsync(SystemRoleConstants.AdminId);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Forbidden, result.Error);
        roleService.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenRoleIsDeletedReturnsSuccess()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync("role-1"))
            .ReturnsAsync(new Role { Id = "role-1", IsSystem = false });
        roleService.Setup(x => x.DeleteAsync("role-1"))
            .ReturnsAsync(true);

        var result = await CreateService(roleService).DeleteAsync("role-1");

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(ApplicationError.None, result.Error);
        roleService.Verify(x => x.DeleteAsync("role-1"), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenPersistenceFailsReturnsOperationFailed()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(x => x.GetAsync("role-1"))
            .ReturnsAsync(new Role { Id = "role-1", IsSystem = false });
        roleService.Setup(x => x.DeleteAsync("role-1"))
            .ReturnsAsync(false);

        var result = await CreateService(roleService).DeleteAsync("role-1");

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.OperationFailed, result.Error);
        roleService.Verify(x => x.DeleteAsync("role-1"), Times.Once);
    }

    [TestMethod]
    public void GetSupportedPermissions_ReturnsCorePermissionKeys()
    {
        var permissions = CreateService(new Mock<IRoleService>()).GetSupportedPermissions();

        Assert.IsTrue(permissions.Count > 0);
        CollectionAssert.Contains(permissions.ToList(), Functions.App_Read);
        CollectionAssert.Contains(permissions.ToList(), Functions.Config_Publish);
        CollectionAssert.Contains(permissions.ToList(), Functions.Role_Read);
        CollectionAssert.Contains(permissions.ToList(), Functions.Log_Read);
        Assert.AreEqual(permissions.Count, permissions.Distinct().Count());
    }

    private static RoleManagementService CreateService(Mock<IRoleService> roleService)
    {
        return new RoleManagementService(roleService.Object);
    }
}
