using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Application;

public sealed record CreateNodeCommand(string Address, string Remark);

public interface INodeManagementService
{
    Task<IReadOnlyList<ServerNode>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServerNode> GetByAddressAsync(string address, CancellationToken cancellationToken = default);

    Task<ApplicationResult<ServerNode>> CreateAsync(
        CreateNodeCommand command,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult> DeleteAsync(
        string address,
        CancellationToken cancellationToken = default);
}

public interface IPreviewModeAccessor
{
    bool IsPreviewMode { get; }
}
