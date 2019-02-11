using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Common
{
    public class Configuration
    {
        private static IConfiguration _configuration;
        public static IConfiguration Config
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
            }
        }
    }
}
