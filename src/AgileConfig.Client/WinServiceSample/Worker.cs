using AgileConfig.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WinServiceSample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfigClient _client;

        public Worker(ILogger<Worker> logger, IConfigClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                foreach (string key in _client.Data.Keys)
                {
                    var val = _client[key];
                    Console.WriteLine("{0} : {1}", key, val);
                }
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
