using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.ServiceTests.oracle;

public class SettingServiceTests_oracle : SettingServiceTests
{
    private readonly string conn = "user id=x;password=x;data source=192.168.0.123/orcl";

    public override Task<Dictionary<string, string>> GetConfigurationData()
    {
        return
            Task.FromResult(
                new Dictionary<string, string>
                {
                    { "db:provider", "oracle" },
                    { "db:conn", conn }
                });
    }
}