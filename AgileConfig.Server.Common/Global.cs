using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Common
{
    public class Global
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

        private static ILoggerFactory _loggerFactory;
        public static ILoggerFactory LoggerFactory
        {
            get { return _loggerFactory; }
            set
            {
                _loggerFactory = value;
            }
        }
    }
}
