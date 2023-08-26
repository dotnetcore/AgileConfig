using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AgileConfig.Client.RegisterCenter
{
    public static class ConfigClientExtension
    {
        public static IDiscoveryService DiscoveryService(this IConfigClient client)
        {
            return RegisterCenter.DiscoveryService.Instance;
        }
    }
}
