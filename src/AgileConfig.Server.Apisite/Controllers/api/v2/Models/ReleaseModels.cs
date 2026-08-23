using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgileConfig.Server.Apisite.Controllers.api.v2.Models;

public sealed class ReleaseResource
{
    public string Id { get; init; }
    public string ApplicationId { get; init; }
    public string Environment { get; init; }
    public int Version { get; init; }
    public string Log { get; init; }
    public string PublishedBy { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed class CreateReleaseRequest
{
    [MaxLength(100)]
    public string Log { get; init; }

    public List<string> ConfigurationIds { get; init; }
}

public sealed class CreateRollbackRequest
{
    [Required]
    public string ReleaseId { get; init; }
}
