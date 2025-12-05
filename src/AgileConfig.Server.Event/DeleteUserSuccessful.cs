using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event;

public class DeleteUserSuccessful : IEvent
{
    public DeleteUserSuccessful(User user, string userName)
    {
        UserName = userName;
        User = user;
    }

    public string UserName { get; }
    public User User { get; }
}