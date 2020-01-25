using System;
namespace AgileConfig.ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var appId = "xxx";
            var seret = "";
            var host = "http://localhost:5000";

            var provider = new AgileConfig.Client.Configuration.AgileConfigProvider(host, appId, seret, null);
            var result = Console.ReadLine();

            if (result == "c")
            {
                provider.Load();
            }

            Console.ReadLine();
        }
    }
}
