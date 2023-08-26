using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Client;

namespace OnFrameworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hello start .");

            var appid = "test_app";
            var secret = "test_app";
            var nodes = "http://agileconfig_server.xbaby.xyz";
            var client = new ConfigClient(appid, secret, nodes, "DEV");

            client.ConnectAsync().GetAwaiter().GetResult();

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(5000);
                    Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    foreach (string key in client.Data.Keys)
                    {
                        var val = client[key];
                        Console.WriteLine("{0} : {1}", key, val);
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Console.ReadLine();

        }
    }
}
