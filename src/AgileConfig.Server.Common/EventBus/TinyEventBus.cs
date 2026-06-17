using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Common.EventBus;

public class TinyEventBus : ITinyEventBus
{
    private static readonly ConcurrentDictionary<Type, List<Type>> EventHandlerMap = new();
    private readonly ILogger<TinyEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TinyEventBus(IServiceProvider serviceProvider, ILogger<TinyEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Register<T>() where T : class, IEventHandler
    {
        var handlerType = typeof(T);
        var eventType = handlerType.GetInterfaces().FirstOrDefault(x => x.IsGenericType)!.GenericTypeArguments
            .FirstOrDefault();
        if (EventHandlerMap.TryGetValue(eventType, out var handlerTypes))
            handlerTypes.Add(handlerType);
        else
            EventHandlerMap.TryAdd(eventType, [handlerType]);
    }

    /// <summary>
    ///     Trigger an event. This method must be called before the handler is registered.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to fire.</typeparam>
    /// <param name="evt">Event payload instance to dispatch to handlers.</param>
    public void Fire<TEvent>(TEvent evt) where TEvent : IEvent
    {
        _logger.LogInformation("Event fired: {EventType}", typeof(TEvent).Name);

        var eventType = typeof(TEvent);
        if (EventHandlerMap.TryGetValue(eventType, out var handlers))
        {
            if (handlers.Count == 0)
            {
                _logger.LogInformation("Event fired: {EventType}, but no handlers.", typeof(TEvent).Name);
                return;
            }

            foreach (var handlerType in handlers)
                _ = Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType);

                    try
                    {
                        await (handler as IEventHandler)?.Handle(evt)!;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "try run {handler} occur error.", handlerType);
                    }
                });
        }
    }
}