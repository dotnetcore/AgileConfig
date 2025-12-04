using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.ServiceTests.sqlserver;

public class AppServiceTests_sqlserver : AppServiceTests
{
    private readonly string conn =
        "TrustServerCertificate=True;Persist Security Info = False; User ID =dev; Password =dev; Initial Catalog =agile_config_test; Server =.";

    public override Task<Dictionary<string, string>> GetConfigurationData()
    {
        var dict = new Dictionary<string, string>();
        dict["db:provider"] = "sqlserver";
        dict["db:conn"] = conn;


        return Task.FromResult(dict);
    }
}