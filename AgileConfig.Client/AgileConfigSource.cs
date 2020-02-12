using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Client
{
    public static class AgileConfitBuilderExt
    {
        public static IConfigurationBuilder AddAgileConfig(
            this IConfigurationBuilder builder,
            string host, string appId, string secret, ILoggerFactory loggerFactory = null)
        {
            return builder.Add(new AgileConfigSource(host, appId, secret, loggerFactory));
        }
    }

    public class AgileConfigSource : IConfigurationSource
    {
        protected ILoggerFactory LoggerFactory { get; }
        protected string Host { get; }

        protected string AppId { get; }

        protected string Secret { get; }

        public AgileConfigSource(string host, string appId, string secret, ILoggerFactory loggerFactory)
        {
            Host = host;
            AppId = appId;
            Secret = secret;
            LoggerFactory = loggerFactory;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AgileConfigProvider(Host, AppId, Secret, LoggerFactory);
        }
    }
}
