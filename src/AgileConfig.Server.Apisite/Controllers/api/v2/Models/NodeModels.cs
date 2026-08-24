using System;
using System.ComponentModel.DataAnnotations;

namespace AgileConfig.Server.Apisite.Controllers.api.v2.Models;

public sealed class NodeResource
{
    public string Id { get; init; }
    public string Address { get; init; }
    public string Remark { get; init; }
    public string Status { get; init; }
    public DateTime? LastEchoAt { get; init; }
}

public sealed class CreateNodeRequest
{
    [Required, MaxLength(100), Url]
    public string Address { get; init; }

    [MaxLength(50)]
    public string Remark { get; init; }
}
