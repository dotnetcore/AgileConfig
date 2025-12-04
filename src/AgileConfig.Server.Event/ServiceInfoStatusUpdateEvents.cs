using AgileConfig.Server.Common.EventBus;

namespace AgileConfig.Server.Event;

public class ServiceRegisteredEvent : IEvent
{
    public ServiceRegisteredEvent(string uniqueId)
    {
        UniqueId = uniqueId;
    }

    public string UniqueId { get; }
}

public class ServiceUnRegisterEvent : IEvent
{
    public ServiceUnRegisterEvent(string uniqueId)
    {
        UniqueId = uniqueId;
    }

    public string UniqueId { get; }
}

public class ServiceStatusUpdateEvent : IEvent
{
    public ServiceStatusUpdateEvent(string uniqueId)
    {
        UniqueId = uniqueId;
    }

    public string UniqueId { get; }
}