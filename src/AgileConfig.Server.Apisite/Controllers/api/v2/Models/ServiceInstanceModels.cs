using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgileConfig.Server.Apisite.Controllers.api.v2.Models;

public sealed class ServiceInstanceResource
{
    public string Id { get; init; }
    public string ServiceId { get; init; }
    public string Name { get; init; }
    public string IpAddress { get; init; }
    public int? Port { get; init; }
    public IReadOnlyList<string> Metadata { get; init; } = [];
    public string Status { get; init; }
    public string HeartbeatMode { get; init; }
    public string HealthCheckUrl { get; init; }
    public string AlarmUrl { get; init; }
    public DateTime? RegisteredAt { get; init; }
    public DateTime? LastHeartbeatAt { get; init; }
}

public sealed class RegisterServiceInstanceRequest
{
    [Required, MaxLength(100)]
    public string ServiceId { get; init; }

    [Required, MaxLength(100)]
    public string Name { get; init; }

    [Required, MaxLength(100)]
    public string IpAddress { get; init; }

    [Range(1, 65535)]
    public int? Port { get; init; }

    public List<string> Metadata { get; init; } = [];

    [Required, RegularExpression("^(client|server|none)$")]
    public string HeartbeatMode { get; init; }

    [MaxLength(2000)]
    public string HealthCheckUrl { get; init; }

    [MaxLength(2000)]
    public string AlarmUrl { get; init; }
}

public sealed class HeartbeatResource
{
    public string ServiceInstanceId { get; init; }
    public DateTime ReceivedAt { get; init; }
    public string ServicesVersion { get; init; }
}
