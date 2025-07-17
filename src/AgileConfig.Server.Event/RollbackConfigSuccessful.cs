using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event
{
    public class RollbackConfigSuccessful : IEvent
    {
        public RollbackConfigSuccessful(PublishTimeline timelineNode, string userName)
        {
            TimelineNode = timelineNode;
            UserName = userName;
        }

        public PublishTimeline TimelineNode { get; }
        public string UserName { get; }
    }
}
