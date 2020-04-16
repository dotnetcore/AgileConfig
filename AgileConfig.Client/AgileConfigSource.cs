using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agile.Config.Client
{
    public static class AgileConfitBuilderExt
    {
        public static IConfigurationBuilder AddAgileConfig(
            this IConfigurationBuilder builder,
            IConfigClient client)
        {
            return builder.Add(new AgileConfigSource(client));
        }
    }

    public class AgileConfigSource : IConfigurationSource
    {
        protected IConfigClient ConfigClient { get; }

        public AgileConfigSource(IConfigClient client)
        {
            ConfigClient = client;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AgileConfigProvider(ConfigClient);
        }
    }
}
