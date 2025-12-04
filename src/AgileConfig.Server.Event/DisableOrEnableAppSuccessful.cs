using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event;

public class DisableOrEnableAppSuccessful : IEvent
{
    public DisableOrEnableAppSuccessful(App app, string userName)
    {
        App = app;
        UserName = userName;
    }

    public App App { get; }
    public string UserName { get; }
}