using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AgileConfig.Server.Apisite.Utilites
{
    public class IPExt
    {
        public static string[] GetEndpointIp()
        {
            var addressIpv4Hosts = NetworkInterface.GetAllNetworkInterfaces()

          .OrderByDescending(c => c.Speed)
          .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);

            var ips = new List<string>();
            foreach (var item in addressIpv4Hosts)
            {
                if (item.Supports(NetworkInterfaceComponent.IPv4))
                {
                    var props = item.GetIPProperties();
                    //this is ip for ipv4
                    var firstIpV4Address = props.UnicastAddresses
                        .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(c => c.Address)
                        .FirstOrDefault().ToString();
                    ips.Add(firstIpV4Address);
                }
            }
            return ips.ToArray();
        }
    }
}
