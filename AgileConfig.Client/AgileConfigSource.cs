using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agile.Config.Client
{
    public static class AgileConfitBuilderExt
    {
        public static IConfigurationBuilder AddAgileConfig(
            this IConfigurationBuilder builder,
            IConfigClient client, ILoggerFactory loggerFactory = null)
        {
            return builder.Add(new AgileConfigSource(client, loggerFactory));
        }
    }

    public class AgileConfigSource : IConfigurationSource
    {
        protected ILoggerFactory LoggerFactory { get; }
        protected IConfigClient ConfigClient { get; }

        public AgileConfigSource(IConfigClient client, ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            ConfigClient = client;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AgileConfigProvider(ConfigClient, LoggerFactory);
        }
    }
}
