using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Application;

public sealed record RegisterServiceInstanceCommand(
    string ServiceId,
    string ServiceName,
    string Ip,
    int? Port,
    string MetadataJson,
    string CheckUrl,
    string AlarmUrl,
    string HeartBeatMode,
    RegisterWay RegisterWay,
    bool RejectExisting,
    bool AcceptMissingIdentifier = false);

public sealed record ServiceInstanceRegistration(string UniqueId, ServiceInfo Instance, bool WasExisting);

public sealed record HeartbeatReceipt(string ServiceInstanceId, DateTime ReceivedAt, string ServicesVersion);

public interface IServiceInstanceManagementService
{
    Task<IReadOnlyList<ServiceInfo>> GetAllAsync(
        ServiceStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<ServiceInfo>> GetByIdAsync(
        string uniqueId,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<ServiceInstanceRegistration>> RegisterAsync(
        RegisterServiceInstanceCommand command,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult> UnregisterAsync(
        string uniqueId,
        string fallbackServiceId = null,
        bool succeedWhenRemovalFails = false,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<HeartbeatReceipt>> ReceiveHeartbeatAsync(
        string uniqueId,
        CancellationToken cancellationToken = default);
}
