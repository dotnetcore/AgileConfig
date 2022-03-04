using System;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Models;

public class ServiceInfoVM
{
    public string Id { get; set; }

    public string ServiceId { get; set; }

    public string ServiceName { get; set; }

    public string Ip { get; set; }

    public int? Port { get; set; }

    public string MetaData { get; set; }

    public ServiceAlive Alive { get; set; }

    public DateTime? RegisterTime { get; set; }

    public DateTime? LastHeartBeat { get; set; }

    public string HeartBeatMode { get; set; }
}