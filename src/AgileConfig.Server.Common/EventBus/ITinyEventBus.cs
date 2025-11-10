using System.Threading.Tasks;

namespace AgileConfig.Server.Common.EventBus;

public interface IEvent
{
}

public interface IEventHandler<TEvent> : IEventHandler where TEvent : IEvent
{
}

public interface IEventHandler
{
    Task Handle(IEvent evt);
}

public interface ITinyEventBus
{
    void Fire<TEvent>(TEvent evt) where TEvent : IEvent;

    void Register<T>()
        where T : class, IEventHandler;
}