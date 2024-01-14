using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using System.Text;
using AgileConfig.Server.Data.Freesql;
using FreeSql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Repository.Freesql;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.Service.Tests.PostgreSQL
{
    [TestClass()]
    public class ServerNodeServiceTests_pg: ServerNodeServiceTests
    {
        string conn = "Host=192.168.0.125;Port=15432;Database=agileconfig;Username=postgres;Password=123456";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "npgsql";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}