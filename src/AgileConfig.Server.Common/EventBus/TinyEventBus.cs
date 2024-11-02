using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common.EventBus
{
    public class TinyEventBus : ITinyEventBus
    {
        private readonly IServiceCollection _serviceCollection;
        private static readonly ConcurrentDictionary<Type, List<Type>> EventHandlerMap = new ();
        private IServiceProvider _localServiceProvider;
        private readonly ILogger _logger;

        public TinyEventBus(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _logger = _serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>().CreateLogger<TinyEventBus>();
        }
        public void Register<T>() where T : class, IEventHandler
        {
            var handlerType = typeof(T);
            var eventType = handlerType.GetInterfaces().FirstOrDefault(x => x.IsGenericType)!.GenericTypeArguments.FirstOrDefault();
            if (EventHandlerMap.TryGetValue(eventType, out List<Type> handlerTypes))
            {
                handlerTypes.Add(handlerType);
            }
            else
            {
                EventHandlerMap.TryAdd(eventType, [handlerType]);
            }
            _serviceCollection.AddScoped<T>();

        }

        /// <summary>
        /// Trigger an event. This method must be called before the handler is registered.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="evt"></param>
        public void Fire<TEvent>(TEvent evt) where TEvent : IEvent
        {
            _localServiceProvider ??= _serviceCollection.BuildServiceProvider();

            _logger.LogInformation($"Event fired: {typeof(TEvent).Name}");

            var eventType = typeof(TEvent);
            if (EventHandlerMap.TryGetValue(eventType, out List<Type> handlers))
            {
                if (handlers.Count == 0)
                {
                    _logger.LogInformation($"Event fired: {typeof(TEvent).Name}, but no handlers.");
                    return;
                }

                foreach (var handlerType in handlers)
                {
                    _ = Task.Run(async () =>
                    {
                        using var sc = _localServiceProvider.CreateScope();
                        var handler = sc.ServiceProvider.GetService(handlerType);
                        
                        try
                        {
                            await (handler as IEventHandler)?.Handle(evt)!;
                        }
                        catch (Exception ex)
                        {
                            _logger
                                .LogError(ex, "try run {handler} occur error.", handlerType);
                        }
                    });
                }
            }
        }
    }
}
