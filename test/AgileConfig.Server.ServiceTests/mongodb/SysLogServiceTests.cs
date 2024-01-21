using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Repository.Mongodb;
using AgileConfig.Server.IService;
using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.ServiceTests.mongodb;

[TestClass()]
public class SysLogServiceTests_mongo : SysLogServiceTests
{
    public override void ClearData()
    {
        var repository = new SysLogRepository(conn);
       var syslogs = repository.AllAsync().Result;
        foreach (var log in syslogs)
        {
            repository.DeleteAsync(log).Wait();
        }
    }

    string conn = "mongodb://192.168.0.125:27017/agile_config_1";

    public override Dictionary<string, string> GetConfigurationData()
    {
        var dict = base.GetConfigurationData();
        dict["db:provider"] = "mongodb";
        dict["db:conn"] = conn;

        return dict;
    }
}