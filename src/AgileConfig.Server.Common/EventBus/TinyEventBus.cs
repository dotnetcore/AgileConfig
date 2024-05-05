using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common.EventBus
{
    public class TinyEventBus : ITinyEventBus
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly static Dictionary<Type, List<Type>> _eventHandlerMap = new Dictionary<Type, List<Type>>();

        public TinyEventBus(IServiceCollection serviceCollection)
        {
            this._serviceCollection = serviceCollection;
        }
        public void Register<T>() where T : class, IEventHandler
        {
            var handlerType = typeof(T);
            var eventType = handlerType.GetInterfaces().FirstOrDefault(x => x.IsGenericType).GenericTypeArguments.FirstOrDefault();
            if (_eventHandlerMap.TryGetValue(eventType, out List<Type> handlerTypes))
            {
                handlerTypes.Add(handlerType);
            }
            else
            {
                _eventHandlerMap.Add(eventType, new List<Type> {
                    handlerType
                });
            }
            _serviceCollection.AddScoped<T>();

        }

        public void Fire<TEvent>(TEvent evt) where TEvent : IEvent
        {
            using var scope = _serviceCollection.BuildServiceProvider().CreateScope();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<TinyEventBus>();

            logger.LogInformation($"Event fired: {typeof(TEvent).Name}");

            var eventType = typeof(TEvent);
            if (_eventHandlerMap.TryGetValue(eventType, out List<Type> handlers))
            {
                foreach (var handlerType in handlers)
                {
                    _ = Task.Run(async () =>
                    {
                        using (var scope = _serviceCollection.BuildServiceProvider().CreateScope())
                        {
                            var handler = scope.ServiceProvider.GetService(handlerType);
                            if (handler != null)
                            {
                                var handlerIntance = handler as IEventHandler;
                                try
                                {
                                    await handlerIntance.Handle(evt);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "try run {handler} occur error.", handlerType);
                                }
                            }
                        }
                    });
                }
                if (handlers.Count == 0)
                {
                    logger.LogInformation($"Event fired: {typeof(TEvent).Name}, but no handlers.");
                }
            }
        }
    }
}
