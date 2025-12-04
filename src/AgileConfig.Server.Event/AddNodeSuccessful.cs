using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event;

public class AddNodeSuccessful : IEvent
{
    public AddNodeSuccessful(ServerNode node, string userName)
    {
        Node = node;
        UserName = userName;
    }

    public ServerNode Node { get; }
    public string UserName { get; }
}