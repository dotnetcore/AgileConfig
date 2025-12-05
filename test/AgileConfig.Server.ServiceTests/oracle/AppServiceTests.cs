using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.ServiceTests.oracle;

public class AppServiceTests_oracle : AppServiceTests
{
    private readonly string conn = "user id=x;password=x;data source=192.168.0.123/orcl";


    public override Task<Dictionary<string, string>> GetConfigurationData()
    {
        var dict = new Dictionary<string, string>();
        dict["db:provider"] = "oracle";
        dict["db:conn"] = conn;


        return Task.FromResult(dict);
    }
}