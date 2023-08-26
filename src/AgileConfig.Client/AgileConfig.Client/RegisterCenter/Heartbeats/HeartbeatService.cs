using AgileConfig.Client.MessageHandlers;
using AgileConfig.Protocol;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client.RegisterCenter.Heartbeats
{
    class HeartbeatService
    {
        IConfigClient _client;
        HeartbeatChannelPicker _picker;
        ILogger _logger;

        private int Interval
        {
            get
            {
                if (_client.Options == null || _client.Options.RegisterInfo == null || _client.Options.RegisterInfo.Interval < 1)
                {
                    return 30;
                }

                return _client.Options.RegisterInfo.Interval;
            }
        }

        public HeartbeatService(IConfigClient client, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HeartbeatService>();
            _client = client;
            _picker = new HeartbeatChannelPicker(client, loggerFactory);
        }

        public void Start(Func<string> getId)
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var uniqueId = getId();
                    if (!string.IsNullOrEmpty(uniqueId))
                    {
                        var channel = _picker.Pick();
                        await channel.SendAsync(uniqueId);
                    }

                    await Task.Delay(1000 * Interval);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
