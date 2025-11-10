using AgileConfig.Server.Common.EventBus;

namespace AgileConfig.Server.Event;

public class LoginEvent : IEvent
{
    public LoginEvent(string userName)
    {
        UserName = userName;
    }

    public string UserName { get; }
}