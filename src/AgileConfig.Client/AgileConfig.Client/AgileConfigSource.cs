using AgileConfig.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.AspNetCore.Hosting
{
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
