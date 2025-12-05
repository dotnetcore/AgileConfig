using AgileConfig.Server.Common.EventBus;

namespace AgileConfig.Server.Event;

public class ResetUserPasswordSuccessful : IEvent
{
    public ResetUserPasswordSuccessful(string opUser, string userName)
    {
        OpUser = opUser;
        UserName = userName;
    }

    public string OpUser { get; }
    public string UserName { get; }
}