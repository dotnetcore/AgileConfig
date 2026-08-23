using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Application;

public sealed class ServiceInstanceManagementService : IServiceInstanceManagementService
{
    private readonly IRegisterCenterService _registerCenterService;
    private readonly IServiceInfoService _serviceInfoService;
    private readonly ITinyEventBus _tinyEventBus;
    private readonly TimeProvider _timeProvider;

    public ServiceInstanceManagementService(
        IRegisterCenterService registerCenterService,
        IServiceInfoService serviceInfoService,
        ITinyEventBus tinyEventBus,
        TimeProvider timeProvider)
    {
        _registerCenterService = registerCenterService;
        _serviceInfoService = serviceInfoService;
        _tinyEventBus = tinyEventBus;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ServiceInfo>> GetAllAsync(
        ServiceStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return status switch
        {
            ServiceStatus.Healthy => await _serviceInfoService.GetOnlineServiceInfoAsync(),
            ServiceStatus.Unhealthy => await _serviceInfoService.GetOfflineServiceInfoAsync(),
            _ => await _serviceInfoService.GetAllServiceInfoAsync()
        };
    }

    public async Task<ApplicationResult<ServiceInfo>> GetByIdAsync(
        string uniqueId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var instance = string.IsNullOrWhiteSpace(uniqueId)
            ? null
            : await _serviceInfoService.GetByUniqueIdAsync(uniqueId);
        return instance == null
            ? ApplicationResult<ServiceInfo>.Failure(ApplicationError.NotFound)
            : ApplicationResult<ServiceInfo>.Success(instance);
    }

    public async Task<ApplicationResult<ServiceInstanceRegistration>> RegisterAsync(
        RegisterServiceInstanceCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null) return ApplicationResult<ServiceInstanceRegistration>.Failure(ApplicationError.ValidationFailed);

        cancellationToken.ThrowIfCancellationRequested();
        var existing = await _serviceInfoService.GetByServiceIdAsync(command.ServiceId);
        if (existing != null && command.RejectExisting)
            return ApplicationResult<ServiceInstanceRegistration>.Failure(ApplicationError.Conflict);

        var service = new ServiceInfo
        {
            ServiceId = command.ServiceId,
            ServiceName = command.ServiceName,
            Ip = command.Ip,
            Port = command.Port,
            MetaData = command.MetadataJson,
            CheckUrl = command.CheckUrl,
            AlarmUrl = command.AlarmUrl,
            HeartBeatMode = command.HeartBeatMode,
            RegisterWay = command.RegisterWay
        };

        var uniqueId = await _registerCenterService.RegisterAsync(service);
        if (string.IsNullOrWhiteSpace(uniqueId))
        {
            if (!command.AcceptMissingIdentifier)
                return ApplicationResult<ServiceInstanceRegistration>.Failure(ApplicationError.OperationFailed);

            _tinyEventBus.Fire(new ServiceRegisteredEvent(uniqueId));
            return ApplicationResult<ServiceInstanceRegistration>.Success(
                new ServiceInstanceRegistration(uniqueId, service, existing != null));
        }

        _tinyEventBus.Fire(new ServiceRegisteredEvent(uniqueId));

        var registered = await _serviceInfoService.GetByUniqueIdAsync(uniqueId) ?? service;
        registered.Id ??= uniqueId;

        return ApplicationResult<ServiceInstanceRegistration>.Success(
            new ServiceInstanceRegistration(uniqueId, registered, existing != null));
    }

    public async Task<ApplicationResult> UnregisterAsync(
        string uniqueId,
        string fallbackServiceId = null,
        bool succeedWhenRemovalFails = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existing = string.IsNullOrWhiteSpace(uniqueId)
            ? null
            : await _serviceInfoService.GetByUniqueIdAsync(uniqueId);
        if (existing == null) return ApplicationResult.Failure(ApplicationError.NotFound);

        var removed = await _registerCenterService.UnRegisterAsync(uniqueId);
        if (!removed && !string.IsNullOrWhiteSpace(fallbackServiceId))
            removed = await _registerCenterService.UnRegisterByServiceIdAsync(fallbackServiceId);

        if (!removed && !succeedWhenRemovalFails)
            return ApplicationResult.Failure(ApplicationError.OperationFailed);

        _tinyEventBus.Fire(new ServiceUnRegisterEvent(existing.Id));
        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult<HeartbeatReceipt>> ReceiveHeartbeatAsync(
        string uniqueId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(uniqueId) ||
            !await _registerCenterService.ReceiveHeartbeatAsync(uniqueId))
            return ApplicationResult<HeartbeatReceipt>.Failure(ApplicationError.NotFound);

        var servicesVersion = await _serviceInfoService.ServicesMD5Cache();
        return ApplicationResult<HeartbeatReceipt>.Success(new HeartbeatReceipt(
            uniqueId,
            _timeProvider.GetUtcNow().UtcDateTime,
            servicesVersion));
    }
}
