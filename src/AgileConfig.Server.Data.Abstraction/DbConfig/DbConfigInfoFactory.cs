using AgileConfig.Server.Common;
using System.Collections.Concurrent;

namespace AgileConfig.Server.Data.Abstraction.DbProvider
{
    public static class DbConfigInfoFactory
    {
        private static ConcurrentDictionary<string, IDbConfigInfo> _envProviders = new ConcurrentDictionary<string, IDbConfigInfo>();
        private static IDbConfigInfo _default { get; }

        static DbConfigInfoFactory()
        {
            var providerPath = $"db:provider";
            var connPath = $"db:conn";

            var providerValue = Global.Config[providerPath];
            var connValue = Global.Config[connPath];

            if (string.IsNullOrEmpty(providerValue))
            {
                throw new ArgumentNullException(providerPath);
            }
            if (string.IsNullOrEmpty(connValue))
            {
                throw new ArgumentNullException(connPath);
            }

            var configInfo = new DbConfigInfo("", providerValue, connValue);
            _default = configInfo;
            _envProviders.TryAdd(providerPath, configInfo);
        }

        public static IDbConfigInfo GetConfigInfo(string env = "")
        {
            if (string.IsNullOrEmpty(env))
            {
                return _default;
            }

            var providerPath = $"";
            var connPath = $"";
            providerPath = $"db:env:{env}:provider";
            connPath = $"db:env:{env}:conn";

            _envProviders.TryGetValue(providerPath, out IDbConfigInfo configInfo);

            if (configInfo != null)
            {
                return configInfo;
            }

            var providerValue = Global.Config[providerPath];
            var connValue = Global.Config[connPath];

            if (string.IsNullOrEmpty(providerValue))
            {
                return _default;
            }
            if (string.IsNullOrEmpty(connValue))
            {
                return _default;
            }

            configInfo = new DbConfigInfo(env, providerValue, connValue);
            _envProviders.TryAdd(providerPath, configInfo);

            return configInfo;
        }
    }
}
