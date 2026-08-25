using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite;
using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using AgileConfig.Server.OIDC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
public sealed class AdminControllerCoverageTests
{
    [TestMethod]
    public async Task Login_HandlesEmptyInvalidAndSuccessfulCredentials()
    {
        SetConfig();
        var userService = new Mock<IUserService>();
        var permissionService = new Mock<IPermissionService>();
        var jwtService = new Mock<IJwtService>();
        var eventBus = new Mock<ITinyEventBus>();
        var controller = CreateController(userService, permissionService, jwtService, eventBus);

        var empty = AsJson(await controller.Login4AntdPro(new LoginVM { userName = "admin", password = "" }));
        Assert.AreEqual("error", Read<string>(empty, "status"));
        Assert.AreEqual(Messages.PasswordCannotBeEmpty, Read<string>(empty, "message"));
        userService.Verify(x => x.ValidateUserPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        userService.Setup(x => x.ValidateUserPassword("admin", "bad")).ReturnsAsync(false);
        var invalid = AsJson(await controller.Login4AntdPro(new LoginVM { userName = "admin", password = "bad" }));
        Assert.AreEqual("error", Read<string>(invalid, "status"));
        Assert.AreEqual(Messages.PasswordError, Read<string>(invalid, "message"));

        var user = new User { Id = "admin-id", UserName = "admin", Status = UserStatus.Normal };
        userService.Setup(x => x.ValidateUserPassword("admin", "good")).ReturnsAsync(true);
        userService.Setup(x => x.GetUsersByNameAsync("admin")).ReturnsAsync(new List<User> { user });
        userService.Setup(x => x.GetUserRolesAsync("admin-id")).ReturnsAsync(new List<Role>
        {
            new() { Id = SystemRoleConstants.AdminId, Name = "Administrator" },
            new() { Id = SystemRoleConstants.OperatorId, Name = "Operator" }
        });
        permissionService.Setup(x => x.GetUserPermission("admin-id")).ReturnsAsync(new List<string> { Functions.User_Read });
        jwtService.Setup(x => x.GetToken("admin-id", "admin", true)).Returns("jwt-token");

        var success = AsJson(await controller.Login4AntdPro(new LoginVM { userName = "admin", password = "good" }));
        Assert.AreEqual("ok", Read<string>(success, "status"));
        Assert.AreEqual("jwt-token", Read<string>(success, "token"));
        Assert.AreEqual("Bearer", Read<string>(success, "type"));
        CollectionAssert.AreEqual(new[] { "Administrator", "Operator" },
            ((IEnumerable<string>)Read<object>(success, "currentAuthority")).ToArray());
        CollectionAssert.AreEqual(new[] { Functions.User_Read },
            ((IEnumerable<string>)Read<object>(success, "currentFunctions")).ToArray());
        eventBus.Verify(x => x.Fire(It.Is<LoginEvent>(evt => evt.UserName == "admin")), Times.Once);
    }

    [TestMethod]
    public async Task OidcLogin_RejectsDisabledAndInvalidTokens()
    {
        var oidcClient = new Mock<IOidcClient>();
        var controller = CreateController(new Mock<IUserService>(), new Mock<IPermissionService>(),
            new Mock<IJwtService>(), new Mock<ITinyEventBus>(), oidcClient);

        SetConfig(ssoEnabled: false);
        var disabled = await controller.OidcLoginByCode("code");
        Assert.IsInstanceOfType(disabled, typeof(BadRequestObjectResult));
        Assert.AreEqual("SSO not enabled", ((BadRequestObjectResult)disabled).Value);
        oidcClient.Verify(x => x.Validate(It.IsAny<string>()), Times.Never);

        SetConfig(ssoEnabled: true);
        oidcClient.Setup(x => x.Validate("empty")).ReturnsAsync(("", "access"));
        var empty = AsJson(await controller.OidcLoginByCode("empty"));
        Assert.IsFalse(Read<bool>(empty, "success"));
        Assert.AreEqual("Code validate failed", Read<string>(empty, "message"));

        oidcClient.Setup(x => x.Validate("invalid")).ReturnsAsync(("token", "access"));
        oidcClient.Setup(x => x.UnboxIdToken("token")).Returns(("", "user"));
        var invalidId = AsJson(await controller.OidcLoginByCode("invalid"));
        Assert.IsFalse(Read<bool>(invalidId, "success"));
        Assert.AreEqual("IdToken invalid", Read<string>(invalidId, "message"));

        oidcClient.Setup(x => x.UnboxIdToken("token")).Returns(("id", ""));
        var invalidName = AsJson(await controller.OidcLoginByCode("invalid"));
        Assert.IsFalse(Read<bool>(invalidName, "success"));
        Assert.AreEqual("IdToken invalid", Read<string>(invalidName, "message"));
    }

    [TestMethod]
    public async Task OidcLogin_CreatesFirstUserAndRejectsDeletedUser()
    {
        SetConfig(ssoEnabled: true);
        var oidcClient = new Mock<IOidcClient>();
        var userService = new Mock<IUserService>();
        var permissionService = new Mock<IPermissionService>();
        var jwtService = new Mock<IJwtService>();
        var eventBus = new Mock<ITinyEventBus>();
        var controller = CreateController(userService, permissionService, jwtService, eventBus, oidcClient);
        oidcClient.Setup(x => x.Validate("first")).ReturnsAsync(("id-token", "access"));
        oidcClient.Setup(x => x.UnboxIdToken("id-token")).Returns(("oidc-id", "oidc-user"));

        User added = null;
        userService.SetupSequence(x => x.GetUsersByNameAsync("oidc-user"))
            .ReturnsAsync(new List<User>())
            .ReturnsAsync(() => new List<User> { added });
        userService.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => added = user)
            .ReturnsAsync(true);
        userService.Setup(x => x.UpdateUserRolesAsync("oidc-id", It.IsAny<List<string>>())).ReturnsAsync(true);
        userService.Setup(x => x.GetUserRolesAsync("oidc-id")).ReturnsAsync(new List<Role>
        {
            new() { Id = SystemRoleConstants.OperatorId, Name = "Operator" }
        });
        permissionService.Setup(x => x.GetUserPermission("oidc-id")).ReturnsAsync(new List<string>());
        jwtService.Setup(x => x.GetToken("oidc-id", "oidc-user", false)).Returns("oidc-jwt");

        var firstLogin = AsJson(await controller.OidcLoginByCode("first"));
        Assert.AreEqual("ok", Read<string>(firstLogin, "status"));
        Assert.AreEqual("oidc-jwt", Read<string>(firstLogin, "token"));
        Assert.IsNotNull(added);
        Assert.AreEqual("oidc-id", added.Id);
        Assert.AreEqual(UserSource.SSO, added.Source);
        Assert.AreEqual(UserStatus.Normal, added.Status);
        userService.Verify(x => x.UpdateUserRolesAsync("oidc-id",
            It.Is<List<string>>(roles => roles.SequenceEqual(new[] { SystemRoleConstants.OperatorId }))), Times.Once);
        eventBus.Verify(x => x.Fire(It.Is<LoginEvent>(evt => evt.UserName == "oidc-user")), Times.Once);

        var deleted = new User { Id = "deleted-id", UserName = "deleted", Status = UserStatus.Deleted };
        oidcClient.Setup(x => x.Validate("deleted")).ReturnsAsync(("deleted-token", "access"));
        oidcClient.Setup(x => x.UnboxIdToken("deleted-token")).Returns(("deleted-id", "deleted"));
        userService.Setup(x => x.GetUsersByNameAsync("deleted")).ReturnsAsync(new List<User> { deleted });

        var deletedResult = AsJson(await controller.OidcLoginByCode("deleted"));
        Assert.AreEqual("error", Read<string>(deletedResult, "status"));
        Assert.AreEqual(Messages.UserDeleted, Read<string>(deletedResult, "message"));
    }

    [TestMethod]
    public async Task PasswordInitialization_ReportsStateAndValidationOutcomes()
    {
        SetConfig();
        var system = new Mock<ISystemInitializationService>();
        var eventBus = new Mock<ITinyEventBus>();
        var controller = CreateController(new Mock<IUserService>(), new Mock<IPermissionService>(),
            new Mock<IJwtService>(), eventBus, system: system);

        system.SetupSequence(x => x.HasSa()).Returns(false).Returns(true);
        var state = AsJson(controller.PasswordInited());
        Assert.IsTrue(Read<bool>(state, "success"));
        Assert.IsFalse(Read<bool>(state, "data"));

        var empty = AsJson(controller.InitPassword(new InitPasswordVM { password = "", confirmPassword = "" }));
        Assert.IsFalse(Read<bool>(empty, "success"));
        Assert.AreEqual(Messages.PasswordCannotBeEmpty, Read<string>(empty, "message"));

        var tooLong = AsJson(controller.InitPassword(new InitPasswordVM
        {
            password = new string('p', 51),
            confirmPassword = new string('p', 51)
        }));
        Assert.IsFalse(Read<bool>(tooLong, "success"));
        Assert.AreEqual(Messages.PasswordMaxLength50, Read<string>(tooLong, "message"));

        var mismatch = AsJson(controller.InitPassword(new InitPasswordVM { password = "one", confirmPassword = "two" }));
        Assert.IsFalse(Read<bool>(mismatch, "success"));
        Assert.AreEqual(Messages.PasswordMismatch, Read<string>(mismatch, "message"));

        var alreadySet = AsJson(controller.InitPassword(new InitPasswordVM { password = "one", confirmPassword = "one" }));
        Assert.IsFalse(Read<bool>(alreadySet, "success"));
        Assert.AreEqual(Messages.PasswordAlreadySet, Read<string>(alreadySet, "message"));

        system.Setup(x => x.HasSa()).Returns(false);
        system.SetupSequence(x => x.TryInitSaPassword("good-password")).Returns(false).Returns(true);
        var failed = AsJson(controller.InitPassword(new InitPasswordVM
        {
            password = "good-password",
            confirmPassword = "good-password"
        }));
        Assert.IsFalse(Read<bool>(failed, "success"));
        Assert.AreEqual(Messages.InitPasswordFailed, Read<string>(failed, "message"));

        var succeeded = AsJson(controller.InitPassword(new InitPasswordVM
        {
            password = "good-password",
            confirmPassword = "good-password"
        }));
        Assert.IsTrue(Read<bool>(succeeded, "success"));
        eventBus.Verify(x => x.Fire(It.IsAny<InitSaPasswordSuccessful>()), Times.Once);
    }

    [TestMethod]
    public async Task ChangePassword_HandlesPreviewValidationAndPersistenceOutcomes()
    {
        var userService = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var system = new Mock<ISystemInitializationService>();
        var controller = CreateController(userService, new Mock<IPermissionService>(), new Mock<IJwtService>(),
            eventBus, system: system);

        SetConfig(previewMode: true);
        var preview = AsJson(await controller.ChangePassword(new ChangePasswordVM
        {
            oldPassword = "old",
            password = "new",
            confirmPassword = "new"
        }));
        Assert.IsFalse(Read<bool>(preview, "success"));
        Assert.AreEqual(Messages.DemoModeNoPasswordChange, Read<string>(preview, "message"));
        userService.Verify(x => x.ValidateUserPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        SetConfig();
        var emptyOld = AsJson(await controller.ChangePassword(new ChangePasswordVM { oldPassword = "" }));
        Assert.AreEqual("err_resetpassword_01", Read<string>(emptyOld, "err_code"));

        userService.Setup(x => x.ValidateUserPassword("current-user", "bad-old")).ReturnsAsync(false);
        var badOld = AsJson(await controller.ChangePassword(new ChangePasswordVM { oldPassword = "bad-old" }));
        Assert.AreEqual(Messages.OriginalPasswordError, Read<string>(badOld, "message"));
        Assert.AreEqual("err_resetpassword_02", Read<string>(badOld, "err_code"));

        userService.Setup(x => x.ValidateUserPassword("current-user", "old")).ReturnsAsync(true);
        var emptyNew = AsJson(await controller.ChangePassword(new ChangePasswordVM
        {
            oldPassword = "old",
            password = "",
            confirmPassword = ""
        }));
        Assert.AreEqual(Messages.NewPasswordCannotBeEmpty, Read<string>(emptyNew, "message"));

        var tooLong = AsJson(await controller.ChangePassword(new ChangePasswordVM
        {
            oldPassword = "old",
            password = new string('n', 51),
            confirmPassword = new string('n', 51)
        }));
        Assert.AreEqual(Messages.NewPasswordMaxLength50, Read<string>(tooLong, "message"));

        var mismatch = AsJson(await controller.ChangePassword(new ChangePasswordVM
        {
            oldPassword = "old",
            password = "new-one",
            confirmPassword = "new-two"
        }));
        Assert.AreEqual(Messages.NewPasswordMismatch, Read<string>(mismatch, "message"));

        userService.Setup(x => x.GetUsersByNameAsync("current-user"))
            .ReturnsAsync(new List<User> { new() { Id = "deleted", UserName = "current-user", Status = UserStatus.Deleted } });
        var missing = AsJson(await controller.ChangePassword(new ChangePasswordVM
        {
            oldPassword = "old",
            password = "new-one",
            confirmPassword = "new-one"
        }));
        Assert.AreEqual(Messages.UserNotFound, Read<string>(missing, "message"));
        Assert.AreEqual("err_resetpassword_06", Read<string>(missing, "err_code"));

        var user = new User { Id = "current-id", UserName = "current-user", Status = UserStatus.Normal, Salt = "salt" };
        userService.Setup(x => x.GetUsersByNameAsync("current-user")).ReturnsAsync(new List<User> { user });
        userService.SetupSequence(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(false).ReturnsAsync(true);
        var failed = AsJson(await controller.ChangePassword(new ChangePasswordVM
        {
            oldPassword = "old",
            password = "new-one",
            confirmPassword = "new-one"
        }));
        Assert.AreEqual(Messages.ChangePasswordFailed, Read<string>(failed, "message"));

        var succeeded = AsJson(await controller.ChangePassword(new ChangePasswordVM
        {
            oldPassword = "old",
            password = "new-two",
            confirmPassword = "new-two"
        }));
        Assert.IsTrue(Read<bool>(succeeded, "success"));
        Assert.AreEqual(Encrypt.Md5("new-two" + user.Salt), user.Password);
        eventBus.Verify(x => x.Fire(It.Is<ChangeUserPasswordSuccessful>(evt => evt.UserName == "current-user")), Times.Once);
    }

    [TestMethod]
    public async Task Logoff_SignsOutAndRedirectsToLogin()
    {
        SetConfig();
        var auth = new Mock<IAuthenticationService>();
        auth.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        var controller = CreateController(new Mock<IUserService>(), new Mock<IPermissionService>(),
            new Mock<IJwtService>(), new Mock<ITinyEventBus>());
        controller.HttpContext.RequestServices = new ServiceCollection()
            .AddSingleton(auth.Object)
            .BuildServiceProvider();

        var result = await controller.Logoff();
        Assert.IsInstanceOfType(result, typeof(RedirectResult));
        Assert.AreEqual("Login", ((RedirectResult)result).Url);
        auth.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>(), null, null), Times.Once);
    }

    private static AdminController CreateController(
        Mock<IUserService> userService,
        Mock<IPermissionService> permissionService,
        Mock<IJwtService> jwtService,
        Mock<ITinyEventBus> eventBus,
        Mock<IOidcClient> oidcClient = null,
        Mock<ISystemInitializationService> system = null)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim("username", "current-user") }, "test"))
        };
        var controller = new AdminController(
            new Mock<ISettingService>().Object,
            userService.Object,
            permissionService.Object,
            jwtService.Object,
            (oidcClient ?? new Mock<IOidcClient>()).Object,
            eventBus.Object,
            (system ?? new Mock<ISystemInitializationService>()).Object);
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    private static void SetConfig(bool ssoEnabled = false, bool previewMode = false)
    {
        Global.Config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["SSO:enabled"] = ssoEnabled.ToString(),
            ["preview_mode"] = previewMode.ToString()
        }).Build();
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
        Assert.IsNotNull(property, $"Missing JSON property '{propertyName}'.");
        return (T)property.GetValue(result.Value);
    }
}
