using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Users;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ApplicationTests;

[TestClass]
public sealed class UserManagementServiceTests
{
    [TestMethod]
    public async Task SearchAsync_FiltersSortsPagesAndLoadsRolesForCurrentPage()
    {
        var userService = new Mock<IUserService>();
        var now = new DateTime(2026, 1, 1);
        userService.Setup(x => x.GetAll()).ReturnsAsync(new List<User>
        {
            new() { Id = "older", UserName = "alice", Team = "platform", Status = UserStatus.Normal, CreateTime = now.AddDays(-1) },
            new() { Id = "newer", UserName = "alice-2", Team = "platform", Status = UserStatus.Normal, CreateTime = now },
            new() { Id = "other", UserName = "bob", Team = "platform", Status = UserStatus.Normal, CreateTime = now.AddDays(1) },
            new() { Id = "deleted", UserName = "alice", Team = "platform", Status = UserStatus.Deleted, CreateTime = now },
            new() { Id = SystemSettings.SuperAdminId, UserName = "alice-admin", Team = "platform", Status = UserStatus.Normal, CreateTime = now }
        });
        userService.Setup(x => x.GetUserRolesAsync("older"))
            .ReturnsAsync(new List<Role> { new() { Id = "operator", Name = "Operator" } });

        var result = await CreateService(userService).SearchAsync(
            new SearchUsersQuery("alice", "platform", 2, 1));

        Assert.AreEqual(2, result.Total);
        Assert.AreEqual(2, result.Current);
        Assert.AreEqual(1, result.PageSize);
        Assert.AreEqual("older", result.Users.Single().User.Id);
        CollectionAssert.AreEqual(new[] { "operator" }, result.Users.Single().RoleIds.ToArray());
        userService.Verify(x => x.GetUserRolesAsync("older"), Times.Once);
        userService.Verify(x => x.GetUserRolesAsync(It.Is<string>(id => id != "older")), Times.Never);
    }

    [TestMethod]
    public async Task CreateAsync_WhenActiveUserExists_ReturnsConflictWithoutWriting()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetUsersByNameAsync("alice"))
            .ReturnsAsync(new List<User> { new() { UserName = "alice", Status = UserStatus.Normal } });

        var result = await CreateService(userService).CreateAsync(
            new CreateUserCommand("alice", "secret", "team", Array.Empty<string>()));

        Assert.AreEqual(ApplicationError.Conflict, result.Error);
        userService.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        userService.Verify(x => x.UpdateUserRolesAsync(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateAsync_MapsPasswordTimeRolesAndEvent()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var time = new FixedTimeProvider(new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero));
        User created = null;
        List<string> roles = null;
        userService.Setup(x => x.GetUsersByNameAsync("alice")).ReturnsAsync(new List<User>());
        userService.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(x => created = x)
            .ReturnsAsync(true);
        userService.Setup(x => x.UpdateUserRolesAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
            .Callback<string, List<string>>((_, value) => roles = value)
            .ReturnsAsync(true);

        var result = await CreateService(userService, eventBus, time).CreateAsync(
            new CreateUserCommand("alice", "secret", "platform", new[] { "role-1", "", "role-1" }));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(created, result.Value);
        Assert.AreEqual("alice", created.UserName);
        Assert.AreEqual("platform", created.Team);
        Assert.AreEqual(time.GetLocalNow().DateTime, created.CreateTime);
        Assert.AreEqual(Encrypt.Md5("secret" + created.Salt), created.Password);
        CollectionAssert.AreEqual(new[] { "role-1" }, roles);
        eventBus.Verify(x => x.Fire(It.Is<AddUserSuccessful>(e =>
            ReferenceEquals(e.User, created) && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_UsesOperatorRoleByDefaultAndReportsPartialFailure()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetUsersByNameAsync(It.IsAny<string>())).ReturnsAsync(new List<User>());
        userService.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(true);
        userService.Setup(x => x.UpdateUserRolesAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
            .ReturnsAsync(false);

        var result = await CreateService(userService).CreateAsync(
            new CreateUserCommand("alice", "secret", null, null));

        Assert.AreEqual(ApplicationError.OperationFailed, result.Error);
        userService.Verify(x => x.UpdateUserRolesAsync(result.Value.Id,
            It.Is<List<string>>(roles => roles.SequenceEqual(new[] { SystemRoleConstants.OperatorId }))), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WhenMissing_ReturnsNotFoundWithoutWriting()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetUserAsync("missing")).ReturnsAsync((User)null);

        var result = await CreateService(userService).UpdateAsync(
            new UpdateUserCommand("missing", "team", Array.Empty<string>()));

        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_UpdatesTeamRolesTimeAndEvent()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var time = new FixedTimeProvider(new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero));
        var user = new User { Id = "user-1", UserName = "alice", Team = "old" };
        userService.Setup(x => x.GetUserAsync(user.Id)).ReturnsAsync(user);
        userService.Setup(x => x.UpdateAsync(user)).ReturnsAsync(true);
        userService.Setup(x => x.UpdateUserRolesAsync(user.Id, It.IsAny<List<string>>())).ReturnsAsync(true);

        var result = await CreateService(userService, eventBus, time).UpdateAsync(
            new UpdateUserCommand(user.Id, "new", new[] { "role-1", "role-1" }));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("new", user.Team);
        Assert.AreEqual(time.GetLocalNow().DateTime, user.UpdateTime);
        userService.Verify(x => x.UpdateUserRolesAsync(user.Id,
            It.Is<List<string>>(roles => roles.SequenceEqual(new[] { "role-1" }))), Times.Once);
        eventBus.Verify(x => x.Fire(It.Is<EditUserSuccessful>(e =>
            ReferenceEquals(e.User, user) && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task ResetPasswordAsync_HandlesMissingFailureAndSuccess()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new User { Id = "user-1", UserName = "alice", Salt = "salt" };
        userService.Setup(x => x.GetUserAsync("missing")).ReturnsAsync((User)null);
        userService.Setup(x => x.GetUserAsync(user.Id)).ReturnsAsync(user);
        userService.SetupSequence(x => x.UpdateAsync(user)).ReturnsAsync(false).ReturnsAsync(true);
        var service = CreateService(userService, eventBus);

        Assert.AreEqual(ApplicationError.NotFound, (await service.ResetPasswordAsync("missing")).Error);
        Assert.AreEqual(ApplicationError.OperationFailed, (await service.ResetPasswordAsync(user.Id)).Error);
        Assert.AreEqual(Encrypt.Md5("123456" + user.Salt), user.Password);
        Assert.IsTrue((await service.ResetPasswordAsync(user.Id)).Succeeded);
        eventBus.Verify(x => x.Fire(It.Is<ResetUserPasswordSuccessful>(e =>
            e.OpUser == "operator" && e.UserName == "alice")), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_HandlesMissingFailureAndSuccess()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new User { Id = "user-1", UserName = "alice", Status = UserStatus.Normal };
        userService.Setup(x => x.GetUserAsync("missing")).ReturnsAsync((User)null);
        userService.Setup(x => x.GetUserAsync(user.Id)).ReturnsAsync(user);
        userService.SetupSequence(x => x.UpdateAsync(user)).ReturnsAsync(false).ReturnsAsync(true);
        var service = CreateService(userService, eventBus);

        Assert.AreEqual(ApplicationError.NotFound, (await service.DeleteAsync("missing")).Error);
        Assert.AreEqual(ApplicationError.OperationFailed, (await service.DeleteAsync(user.Id)).Error);
        Assert.AreEqual(UserStatus.Deleted, user.Status);
        Assert.IsTrue((await service.DeleteAsync(user.Id)).Succeeded);
        eventBus.Verify(x => x.Fire(It.Is<DeleteUserSuccessful>(e =>
            ReferenceEquals(e.User, user) && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task DirectoryQueries_FilterAndSortUsers()
    {
        var userService = new Mock<IUserService>();
        var activeUsers = new List<User>
        {
            new() { Id = "z", UserName = "z", Team = "z", Status = UserStatus.Normal },
            new() { Id = "a2", UserName = "z", Team = "a", Status = UserStatus.Normal },
            new() { Id = "a1", UserName = "a", Team = "a", Status = UserStatus.Normal },
            new() { Id = "deleted", UserName = "d", Team = "a", Status = UserStatus.Deleted },
            new() { Id = SystemSettings.SuperAdminId, UserName = "admin", Team = "a", Status = UserStatus.Normal }
        };
        userService.Setup(x => x.GetAll()).ReturnsAsync(activeUsers);
        userService.Setup(x => x.GetUsersByRoleAsync(SystemRoleConstants.AdminId)).ReturnsAsync(activeUsers);
        var service = CreateService(userService);

        var all = await service.GetAllActiveAsync();
        var administrators = await service.GetAdministratorsAsync();

        Assert.AreEqual("a1,a2,z", string.Join(",", all.Select(x => x.Id)));
        Assert.AreEqual(
            $"a1,{SystemSettings.SuperAdminId},a2,z",
            string.Join(",", administrators.Select(x => x.Id)));
    }

    private static UserManagementService CreateService(
        Mock<IUserService> userService,
        Mock<ITinyEventBus> eventBus = null,
        TimeProvider timeProvider = null)
    {
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserName).Returns("operator");
        return new UserManagementService(
            userService.Object,
            (eventBus ?? new Mock<ITinyEventBus>()).Object,
            currentUser.Object,
            timeProvider ?? TimeProvider.System);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}
