using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client.RegisterCenter.Heartbeats
{
    public interface IChannel
    {
        Task SendAsync(string serviceUniqueId);
    }
}
