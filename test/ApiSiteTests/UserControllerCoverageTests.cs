using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
public sealed class UserControllerCoverageTests
{
    [TestMethod]
    public async Task Search_FiltersSortsPagesAndMapsRoles()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var baseTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var users = new List<User>
        {
            new() { Id = "old", UserName = "alice", Team = "zeta", Status = UserStatus.Normal, CreateTime = baseTime.AddMinutes(-10) },
            new() { Id = "new", UserName = "alice", Team = "alpha", Status = UserStatus.Normal, CreateTime = baseTime },
            new() { Id = "null-name", UserName = null, Team = "alpha", Status = UserStatus.Normal, CreateTime = baseTime.AddMinutes(-1) },
            new() { Id = "null-team", UserName = "alice", Team = null, Status = UserStatus.Normal, CreateTime = baseTime.AddMinutes(-2) },
            new() { Id = "deleted", UserName = "alice", Team = "alpha", Status = UserStatus.Deleted, CreateTime = baseTime.AddMinutes(1) },
            new() { Id = SystemSettings.SuperAdminId, UserName = "admin", Team = "root", Status = UserStatus.Normal, CreateTime = baseTime.AddMinutes(2) }
        };
        userService.Setup(x => x.GetAll()).ReturnsAsync(users);
        userService.Setup(x => x.GetUserRolesAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => new List<Role>
            {
                new() { Id = id + "-role", Name = id + " role" }
            });
        var controller = CreateController(userService, eventBus);

        var filtered = AsJson(await controller.Search("alice", "alpha", 1, 10));
        var filteredUsers = Read<IEnumerable<UserVM>>(filtered, "data").ToList();

        Assert.AreEqual(1, Read<int>(filtered, "total"));
        Assert.AreEqual("new", filteredUsers.Single().Id);
        CollectionAssert.AreEqual(new[] { "new-role" }, filteredUsers.Single().UserRoleIds);
        CollectionAssert.AreEqual(new[] { "new role" }, filteredUsers.Single().UserRoleNames);

        var paged = AsJson(await controller.Search(null, null, 2, 2));
        var pagedUsers = Read<IEnumerable<UserVM>>(paged, "data").ToList();

        Assert.AreEqual(2, Read<int>(paged, "current"));
        Assert.AreEqual(2, Read<int>(paged, "pageSize"));
        Assert.AreEqual(4, Read<int>(paged, "total"));
        CollectionAssert.AreEqual(new[] { "null-team", "old" }, pagedUsers.Select(x => x.Id).ToArray());
        userService.Verify(x => x.GetUserRolesAsync(It.IsAny<string>()), Times.Exactly(3));
    }

    [TestMethod]
    public void Search_RejectsInvalidPaging()
    {
        var userService = new Mock<IUserService>();
        var controller = CreateController(userService, new Mock<ITinyEventBus>());

        Assert.ThrowsExactly<ArgumentException>(() => controller.Search(null, null, 0, 20).GetAwaiter().GetResult());
        Assert.ThrowsExactly<ArgumentException>(() => controller.Search(null, null, 1, 0).GetAwaiter().GetResult());
        userService.Verify(x => x.GetAll(), Times.Never);
    }

    [TestMethod]
    public async Task Add_RejectsNullAndExistingNormalUsers()
    {
        var userService = new Mock<IUserService>();
        var controller = CreateController(userService, new Mock<ITinyEventBus>());

        Assert.ThrowsExactly<ArgumentNullException>(() => controller.Add(null).GetAwaiter().GetResult());

        userService.Setup(x => x.GetUsersByNameAsync("already"))
            .ReturnsAsync(new List<User>
            {
                new() { Id = "existing", UserName = "already", Status = UserStatus.Normal }
            });
        var result = AsJson(await controller.Add(new UserVM { UserName = "already", Password = "password" }));

        Assert.IsFalse(Read<bool>(result, "success"));
        StringAssert.Contains(Read<string>(result, "message"), "already");
        userService.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task Add_CreatesUserWithDefaultRoleAndRaisesEvent()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        User addedUser = null;
        List<string> addedRoleIds = null;
        userService.Setup(x => x.GetUsersByNameAsync("new-user"))
            .ReturnsAsync(new List<User>
            {
                new() { Id = "deleted-user", UserName = "new-user", Status = UserStatus.Deleted }
            });
        userService.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => addedUser = user)
            .ReturnsAsync(true);
        userService.Setup(x => x.UpdateUserRolesAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
            .Callback<string, List<string>>((_, roleIds) => addedRoleIds = roleIds)
            .ReturnsAsync(true);
        var controller = CreateController(userService, eventBus, "admin-user");

        var result = AsJson(await controller.Add(new UserVM
        {
            UserName = "new-user",
            Password = "new-password",
            Team = "platform",
            UserRoleIds = null
        }));

        Assert.IsTrue(Read<bool>(result, "success"));
        Assert.IsNotNull(addedUser);
        Assert.AreEqual("new-user", addedUser.UserName);
        Assert.AreEqual("platform", addedUser.Team);
        Assert.AreEqual(UserStatus.Normal, addedUser.Status);
        Assert.IsFalse(string.IsNullOrEmpty(addedUser.Id));
        Assert.AreEqual(Encrypt.Md5("new-password" + addedUser.Salt), addedUser.Password);
        CollectionAssert.AreEqual(new[] { SystemRoleConstants.OperatorId }, addedRoleIds);
        eventBus.Verify(x => x.Fire(It.Is<AddUserSuccessful>(evt =>
            ReferenceEquals(evt.User, addedUser) && evt.UserName == "admin-user")), Times.Once);
    }

    [TestMethod]
    public async Task Add_ReportsAddAndRoleFailures()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        userService.Setup(x => x.GetUsersByNameAsync(It.IsAny<string>())).ReturnsAsync(new List<User>());
        userService.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(false);
        userService.Setup(x => x.UpdateUserRolesAsync(It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(true);
        var controller = CreateController(userService, eventBus);

        var addFailure = AsJson(await controller.Add(new UserVM
        {
            UserName = "failed-add",
            Password = "password",
            UserRoleIds = new List<string> { "role-1", "", "role-1", " " }
        }));
        Assert.IsFalse(Read<bool>(addFailure, "success"));
        Assert.AreEqual(Messages.AddUserFailed, Read<string>(addFailure, "message"));
        eventBus.Verify(x => x.Fire(It.IsAny<AddUserSuccessful>()), Times.Never);

        userService.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(true);
        userService.Setup(x => x.UpdateUserRolesAsync(It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(false);
        var roleFailure = AsJson(await controller.Add(new UserVM
        {
            UserName = "failed-roles",
            Password = "password",
            UserRoleIds = new List<string> { "role-1", "", "role-1", " " }
        }));
        Assert.IsFalse(Read<bool>(roleFailure, "success"));
        Assert.AreEqual(Messages.AddUserFailed, Read<string>(roleFailure, "message"));
        eventBus.Verify(x => x.Fire(It.IsAny<AddUserSuccessful>()), Times.Once);
    }

    [TestMethod]
    public async Task Edit_HandlesNullMissingAndServiceResults()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var existingUser = new User { Id = "user-1", UserName = "user", Team = "old-team", Salt = "salt" };
        userService.Setup(x => x.GetUserAsync("missing")).ReturnsAsync((User)null);
        userService.Setup(x => x.GetUserAsync("user-1")).ReturnsAsync(existingUser);
        userService.SetupSequence(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(true);
        userService.SetupSequence(x => x.UpdateUserRolesAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        var controller = CreateController(userService, eventBus, "editor");

        Assert.ThrowsExactly<ArgumentNullException>(() => controller.Edit(null).GetAwaiter().GetResult());
        var missing = AsJson(await controller.Edit(new UserVM { Id = "missing", Team = "new-team" }));
        Assert.IsFalse(Read<bool>(missing, "success"));
        Assert.AreEqual(Messages.UserNotFoundForOperation, Read<string>(missing, "message"));

        var success = AsJson(await controller.Edit(new UserVM
        {
            Id = "user-1",
            Team = "new-team",
            UserRoleIds = null
        }));
        Assert.IsTrue(Read<bool>(success, "success"));
        Assert.AreEqual("new-team", existingUser.Team);
        Assert.IsNotNull(existingUser.UpdateTime);

        var updateFailure = AsJson(await controller.Edit(new UserVM
        {
            Id = "user-1",
            Team = "another-team",
            UserRoleIds = new List<string> { "role-1", "", "role-1" }
        }));
        Assert.IsFalse(Read<bool>(updateFailure, "success"));
        Assert.AreEqual(Messages.UpdateUserFailed, Read<string>(updateFailure, "message"));

        var roleFailure = AsJson(await controller.Edit(new UserVM
        {
            Id = "user-1",
            Team = "final-team",
            UserRoleIds = new List<string> { "role-1", "", "role-1" }
        }));
        Assert.IsFalse(Read<bool>(roleFailure, "success"));
        eventBus.Verify(x => x.Fire(It.Is<EditUserSuccessful>(evt =>
            ReferenceEquals(evt.User, existingUser) && evt.UserName == "editor")), Times.Exactly(2));
    }

    [TestMethod]
    public async Task ResetPassword_HandlesMissingAndUpdateResults()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new User { Id = "user-1", UserName = "target", Salt = "user-salt", Password = "old" };
        userService.Setup(x => x.GetUserAsync("missing")).ReturnsAsync((User)null);
        userService.Setup(x => x.GetUserAsync("user-1")).ReturnsAsync(user);
        userService.SetupSequence(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(false).ReturnsAsync(true);
        var controller = CreateController(userService, eventBus, "operator");

        Assert.ThrowsExactly<ArgumentNullException>(() => controller.ResetPassword(null).GetAwaiter().GetResult());
        var missing = AsJson(await controller.ResetPassword("missing"));
        Assert.IsFalse(Read<bool>(missing, "success"));
        Assert.AreEqual(Messages.UserNotFoundForOperation, Read<string>(missing, "message"));

        var failed = AsJson(await controller.ResetPassword("user-1"));
        Assert.IsFalse(Read<bool>(failed, "success"));
        Assert.AreEqual(Messages.ResetUserPasswordFailed, Read<string>(failed, "message"));
        Assert.AreEqual(Encrypt.Md5("123456" + user.Salt), user.Password);

        var succeeded = AsJson(await controller.ResetPassword("user-1"));
        Assert.IsTrue(Read<bool>(succeeded, "success"));
        Assert.AreEqual("", Read<string>(succeeded, "message"));
        eventBus.Verify(x => x.Fire(It.Is<ResetUserPasswordSuccessful>(evt =>
            evt.OpUser == "operator" && evt.UserName == "target")), Times.Once);
    }

    [TestMethod]
    public async Task Delete_HandlesMissingAndUpdateResults()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new User { Id = "user-1", UserName = "target", Status = UserStatus.Normal };
        userService.Setup(x => x.GetUserAsync("missing")).ReturnsAsync((User)null);
        userService.Setup(x => x.GetUserAsync("user-1")).ReturnsAsync(user);
        userService.SetupSequence(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(false).ReturnsAsync(true);
        var controller = CreateController(userService, eventBus, "operator");

        Assert.ThrowsExactly<ArgumentNullException>(() => controller.Delete(null).GetAwaiter().GetResult());
        var missing = AsJson(await controller.Delete("missing"));
        Assert.IsFalse(Read<bool>(missing, "success"));
        Assert.AreEqual(Messages.UserNotFoundForOperation, Read<string>(missing, "message"));

        var failed = AsJson(await controller.Delete("user-1"));
        Assert.IsFalse(Read<bool>(failed, "success"));
        Assert.AreEqual(Messages.DeleteUserFailed, Read<string>(failed, "message"));
        Assert.AreEqual(UserStatus.Deleted, user.Status);

        var succeeded = AsJson(await controller.Delete("user-1"));
        Assert.IsTrue(Read<bool>(succeeded, "success"));
        Assert.AreEqual("", Read<string>(succeeded, "message"));
        eventBus.Verify(x => x.Fire(It.Is<DeleteUserSuccessful>(evt =>
            ReferenceEquals(evt.User, user) && evt.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task AdminUsers_FiltersAndSortsNormalAdministrators()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetUsersByRoleAsync(SystemRoleConstants.AdminId))
            .ReturnsAsync(new List<User>
            {
                new() { Id = "z", UserName = "z-user", Team = "z-team", Status = UserStatus.Normal },
                new() { Id = "a2", UserName = "z-user", Team = "a-team", Status = UserStatus.Normal },
                new() { Id = "a1", UserName = "a-user", Team = "a-team", Status = UserStatus.Normal },
                new() { Id = "deleted", UserName = "deleted", Team = "", Status = UserStatus.Deleted }
            });
        var controller = CreateController(userService, new Mock<ITinyEventBus>());

        var result = AsJson(await controller.AdminUsers());
        var users = Read<IEnumerable<UserVM>>(result, "data").ToList();

        Assert.IsTrue(Read<bool>(result, "success"));
        CollectionAssert.AreEqual(new[] { "a1", "a2", "z" }, users.Select(x => x.Id).ToArray());
        userService.Verify(x => x.GetUsersByRoleAsync(SystemRoleConstants.AdminId), Times.Once);
    }

    [TestMethod]
    public async Task AllUsers_FiltersAndSortsNormalNonSuperAdministrators()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetAll()).ReturnsAsync(new List<User>
        {
            new() { Id = SystemSettings.SuperAdminId, UserName = "admin", Team = "root", Status = UserStatus.Normal },
            new() { Id = "deleted", UserName = "deleted", Team = "a-team", Status = UserStatus.Deleted },
            new() { Id = "z", UserName = "z-user", Team = "z-team", Status = UserStatus.Normal },
            new() { Id = "a2", UserName = "z-user", Team = "a-team", Status = UserStatus.Normal },
            new() { Id = "a1", UserName = "a-user", Team = "a-team", Status = UserStatus.Normal }
        });
        var controller = CreateController(userService, new Mock<ITinyEventBus>());

        var result = AsJson(await controller.AllUsers());
        var users = Read<IEnumerable<UserVM>>(result, "data").ToList();

        Assert.IsTrue(Read<bool>(result, "success"));
        CollectionAssert.AreEqual(new[] { "a1", "a2", "z" }, users.Select(x => x.Id).ToArray());
        userService.Verify(x => x.GetAll(), Times.Once);
    }

    private static UserController CreateController(Mock<IUserService> userService,
        Mock<ITinyEventBus> eventBus, string currentUser = "test-user")
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim("username", currentUser) }, "test"))
        };
        var controller = new UserController(userService.Object, eventBus.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    private static JsonResult AsJson(IActionResult result)
    {
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        return (JsonResult)result;
    }

    private static T Read<T>(JsonResult result, string propertyName)
    {
        Assert.IsNotNull(result.Value);
        var property = result.Value.GetType().GetProperty(propertyName);
        Assert.IsNotNull(property);
        return (T)property.GetValue(result.Value);
    }
}
