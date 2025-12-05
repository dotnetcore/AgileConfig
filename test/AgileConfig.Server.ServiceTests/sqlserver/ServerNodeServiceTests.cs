using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.ServiceTests.sqlserver;

public class ServerNodeServiceTests_sqlserver : ServerNodeServiceTests
{
    private readonly string conn =
        "TrustServerCertificate=True;Persist Security Info = False; User ID =dev; Password =dev; Initial Catalog =agile_config_test; Server =.";

    public override Task<Dictionary<string, string>> GetConfigurationData()
    {
        return
            Task.FromResult(
                new Dictionary<string, string>
                {
                    { "db:provider", "sqlserver" },
                    { "db:conn", conn }
                });
    }
}