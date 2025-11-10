using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event;

public class PublishConfigSuccessful : IEvent
{
    public PublishConfigSuccessful(PublishTimeline publishTimeline, string userName)
    {
        PublishTimeline = publishTimeline;
        UserName = userName;
    }

    public PublishTimeline PublishTimeline { get; }
    public string UserName { get; }
}