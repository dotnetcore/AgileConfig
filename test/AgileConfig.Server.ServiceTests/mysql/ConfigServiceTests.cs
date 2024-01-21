using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using AgileConfig.Server.Service.Tests;

namespace AgileConfig.Server.ServiceTests.mysql
{
    [TestClass()]
    public class ConfigServiceTests_mysql : ConfigServiceTests
    {
        string conn = "Database=agile_config_test;Data Source=192.168.0.125;User Id=root;Password=x;port=13306";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "mysql";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}