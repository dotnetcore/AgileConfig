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

public sealed class NodeManagementService : INodeManagementService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IPreviewModeAccessor _previewModeAccessor;
    private readonly IRemoteServerNodeProxy _remoteServerNodeProxy;
    private readonly IServerNodeService _serverNodeService;
    private readonly ITinyEventBus _tinyEventBus;
    private readonly TimeProvider _timeProvider;

    public NodeManagementService(
        IServerNodeService serverNodeService,
        IRemoteServerNodeProxy remoteServerNodeProxy,
        ITinyEventBus tinyEventBus,
        ICurrentUserAccessor currentUserAccessor,
        IPreviewModeAccessor previewModeAccessor,
        TimeProvider timeProvider)
    {
        _serverNodeService = serverNodeService;
        _remoteServerNodeProxy = remoteServerNodeProxy;
        _tinyEventBus = tinyEventBus;
        _currentUserAccessor = currentUserAccessor;
        _previewModeAccessor = previewModeAccessor;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ServerNode>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var nodes = await _serverNodeService.GetAllNodesAsync();
        return nodes.OrderBy(x => x.CreateTime).ToList();
    }

    public Task<ServerNode> GetByAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _serverNodeService.GetAsync(NormalizeAddress(address));
    }

    public async Task<ApplicationResult<ServerNode>> CreateAsync(
        CreateNodeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command == null) return ApplicationResult<ServerNode>.Failure(ApplicationError.ValidationFailed);

        cancellationToken.ThrowIfCancellationRequested();
        var address = NormalizeAddress(command.Address);
        if (string.IsNullOrWhiteSpace(address))
            return ApplicationResult<ServerNode>.Failure(ApplicationError.ValidationFailed);

        var oldNode = await _serverNodeService.GetAsync(address);
        if (oldNode != null)
            return ApplicationResult<ServerNode>.Failure(ApplicationError.Conflict);

        var node = new ServerNode
        {
            Id = address,
            Remark = command.Remark,
            Status = NodeStatus.Offline,
            CreateTime = _timeProvider.GetLocalNow().DateTime
        };

        var added = await _serverNodeService.AddAsync(node);
        if (!added) return ApplicationResult<ServerNode>.Failure(ApplicationError.OperationFailed, node);

        _tinyEventBus.Fire(new AddNodeSuccessful(node, _currentUserAccessor.UserName));
        await _remoteServerNodeProxy.TestEchoAsync(node.Id);

        return ApplicationResult<ServerNode>.Success(node);
    }

    public async Task<ApplicationResult> DeleteAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        if (_previewModeAccessor.IsPreviewMode)
            return ApplicationResult.Failure(ApplicationError.Forbidden);

        cancellationToken.ThrowIfCancellationRequested();
        var normalizedAddress = NormalizeAddress(address);
        var node = string.IsNullOrWhiteSpace(normalizedAddress)
            ? null
            : await _serverNodeService.GetAsync(normalizedAddress);
        if (node == null) return ApplicationResult.Failure(ApplicationError.NotFound);

        var deleted = await _serverNodeService.DeleteAsync(node);
        if (!deleted) return ApplicationResult.Failure(ApplicationError.OperationFailed);

        _tinyEventBus.Fire(new DeleteNodeSuccessful(node, _currentUserAccessor.UserName));
        return ApplicationResult.Success();
    }

    private static string NormalizeAddress(string address)
    {
        return address?.TrimEnd('/');
    }
}
