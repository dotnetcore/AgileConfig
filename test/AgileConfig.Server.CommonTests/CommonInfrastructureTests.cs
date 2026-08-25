using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.RestClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Common.Tests;

[TestClass]
[DoNotParallelize]
public class CommonInfrastructureTests
{
    [TestMethod]
    public void RetryHelpers_RetryUntilSuccessfulAndRethrowFinalFailure()
    {
        var attempts = 0;

        var result = FunctionUtil.TRY(() =>
        {
            attempts++;
            if (attempts < 3) throw new InvalidOperationException();
            return "done";
        }, 3);

        Assert.AreEqual("done", result);
        Assert.AreEqual(3, attempts);
        Assert.ThrowsExactly<InvalidOperationException>(() => FunctionUtil.TRY<int>(() => throw new InvalidOperationException(), 2));

        attempts = 0;
        FunctionUtil.TRY(() =>
        {
            attempts++;
            if (attempts == 1) throw new InvalidOperationException();
        }, 2);
        Assert.AreEqual(2, attempts);
    }

    [TestMethod]
    public async Task AsyncRetryHelpers_RetryRethrowAndSwallowAsSpecified()
    {
        var attempts = 0;
        var result = await FunctionUtil.TRYAsync(async () =>
        {
            attempts++;
            await Task.Yield();
            if (attempts == 1) throw new InvalidOperationException();
            return 42;
        }, 2);

        Assert.AreEqual(42, result);
        Assert.AreEqual(2, attempts);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await FunctionUtil.TRYAsync<int>(() => Task.FromException<int>(new InvalidOperationException()), 1));

        var swallowed = await FunctionUtil.EATAsync<int>(() => Task.FromException<int>(new InvalidOperationException()), 2);
        Assert.AreEqual(0, swallowed);
    }

    [TestMethod]
    public async Task RetryHelpers_RejectNullDelegates()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => FunctionUtil.TRY<string>(null, 1));
        Assert.ThrowsExactly<ArgumentNullException>(() => FunctionUtil.TRY(null, 1));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => FunctionUtil.TRYAsync<string>(null, 1));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => FunctionUtil.EATAsync<string>(null, 1));
    }

    [TestMethod]
    public void BasicAuthorizationExtensions_ParseValidCredentialsAndRejectInvalidHeaders()
    {
        var context = new DefaultHttpContext();
        const string encoded = "YXBwOnNlY3JldDpleHRyYQ==";
        context.Request.Headers.Authorization = "Basic " + encoded;

        Assert.AreEqual(("app", "secret"), context.Request.GetUserNamePasswordFromBasicAuthorization());
        Assert.AreEqual(("app", "secret"), Encrypt.UnboxBasicAuth(context.Request));

        context.Request.Headers.Authorization = "Bearer token";
        Assert.AreEqual(("", ""), context.Request.GetUserNamePasswordFromBasicAuthorization());
        Assert.AreEqual(("", ""), Encrypt.UnboxBasicAuth(context.Request));

        context.Request.Headers.Authorization = "Basic not-base64";
        Assert.AreEqual(("", ""), context.Request.GetUserNamePasswordFromBasicAuthorization());
        Assert.AreEqual(("", ""), Encrypt.UnboxBasicAuth(context.Request));
    }

    [TestMethod]
    public void HttpAndEnvironmentExtensions_ReadClaimsAndQueryEnvironment()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?env=STAGING");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("username", "alice"),
            new Claim("id", "user-1")
        ]));

        var accessor = new HttpContextAccessor { HttpContext = context };
        var envAccessor = new EnvAccessor(accessor);

        Assert.AreEqual("STAGING", envAccessor.Env);
        Assert.AreEqual("alice", context.GetUserNameFromClaim());
        Assert.AreEqual("user-1", context.GetUserIdFromClaim());

        var services = new ServiceCollection();
        Assert.AreSame(services, services.AddEnvAccessor());
        Assert.IsTrue(services.Any(x => x.ServiceType == typeof(IEnvAccessor) && x.Lifetime == ServiceLifetime.Singleton));
    }

    [TestMethod]
    public void EnumExtensions_UseDescriptionWhenPresent()
    {
        Assert.AreEqual("described", DescribedValue.Described.ToDesc());
        Assert.AreEqual(nameof(DescribedValue.Plain), DescribedValue.Plain.ToDesc());
    }

    [TestMethod]
    public async Task TinyEventBus_DispatchesRegisteredHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var bus = new TinyEventBus(services);
        var completion = new TaskCompletionSource<TestEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        TestEventHandler.Completion = completion;

        bus.Register<TestEventHandler>();
        var evt = new TestEvent();
        bus.Fire(evt);

        var completed = await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(2)));
        Assert.AreSame(completion.Task, completed);
        Assert.AreSame(evt, await completion.Task);
    }

    [TestMethod]
    public async Task TinyEventBus_RegistersAdditionalHandlersAndIsolatesHandlerFailures()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var bus = new TinyEventBus(services);
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        ThrowingTestEventHandler.Completion = completion;

        bus.Register<ThrowingTestEventHandler>();
        bus.Register<ThrowingTestEventHandler>();
        bus.Fire(new ThrowingTestEvent());

        var completed = await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(2)));
        Assert.AreSame(completion.Task, completed);
        Assert.IsTrue(await completion.Task);
        await Task.Delay(50);
    }

    [TestMethod]
    public async Task DefaultRestClient_SendsHeadersAndSerializesResponses()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"value\":\"ok\"}", Encoding.UTF8, "application/json")
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var sut = new DefaultRestClient(new StaticHttpClientFactory(client));

        var get = await sut.GetAsync<TestPayload>("/items", new() { ["X-Request-Id"] = "abc" });
        Assert.AreEqual("ok", get.Value);
        Assert.AreEqual(HttpMethod.Get, handler.Request.Method);
        Assert.AreEqual("abc", handler.Request.Headers.GetValues("X-Request-Id").Single());

        var post = await sut.PostAsync<TestPayload>("/items", new { name = "agile" });
        Assert.AreEqual("ok", post.Value);
        Assert.AreEqual(HttpMethod.Post, handler.Request.Method);
        StringAssert.Contains(handler.Body, "agile");
    }

    [TestMethod]
    public async Task ExceptionMiddleware_SetsResponseThenRethrows()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var middleware = new ExceptionHandlerMiddleware(_ => throw new InvalidOperationException("failure"), loggerFactory);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => middleware.Invoke(context));

        Assert.AreEqual(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.AreEqual("text/html", context.Response.ContentType);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        StringAssert.Contains(await reader.ReadToEndAsync(), "500 InternalServerError");
    }

    private enum DescribedValue
    {
        [System.ComponentModel.Description("described")]
        Described,
        Plain
    }

    private sealed class TestEvent : IEvent;

    private sealed class TestEventHandler : IEventHandler<TestEvent>
    {
        public static TaskCompletionSource<TestEvent> Completion { get; set; }

        public Task Handle(IEvent evt)
        {
            Completion.TrySetResult((TestEvent)evt);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingTestEvent : IEvent;

    private sealed class ThrowingTestEventHandler : IEventHandler<ThrowingTestEvent>
    {
        public static TaskCompletionSource<bool> Completion { get; set; }

        public async Task Handle(IEvent evt)
        {
            await Task.Yield();
            Completion.TrySetResult(true);
            throw new InvalidOperationException("handler failure");
        }
    }

    private sealed class TestPayload
    {
        public string Value { get; set; }
    }

    private sealed class StaticHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpRequestMessage Request { get; private set; }
        public string Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseFactory(request);
        }
    }
}
