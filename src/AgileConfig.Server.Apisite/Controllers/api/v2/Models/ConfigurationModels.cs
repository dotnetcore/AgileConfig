using System;
using System.ComponentModel.DataAnnotations;

namespace AgileConfig.Server.Apisite.Controllers.api.v2.Models;

public sealed class ConfigurationResource
{
    public string Id { get; init; }
    public string ApplicationId { get; init; }
    public string Environment { get; init; }
    public string Group { get; init; }
    public string Key { get; init; }
    public string Value { get; init; }
    public string Description { get; init; }
    public string Status { get; init; }
    public string PublicationStatus { get; init; }
    public string ChangeType { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class PublishedConfigurationResource
{
    public string Id { get; init; }
    public string ApplicationId { get; init; }
    public string Environment { get; init; }
    public string Group { get; init; }
    public string Key { get; init; }
    public string Value { get; init; }
}

public sealed class CreateConfigurationRequest
{
    [MaxLength(100)]
    public string Group { get; init; }

    [Required, MaxLength(100)]
    public string Key { get; init; }

    [MaxLength(4000)]
    public string Value { get; init; }

    [MaxLength(200)]
    public string Description { get; init; }
}

public sealed class UpdateConfigurationRequest
{
    [MaxLength(100)]
    public string Group { get; init; }

    [Required, MaxLength(100)]
    public string Key { get; init; }

    [MaxLength(4000)]
    public string Value { get; init; }

    [MaxLength(200)]
    public string Description { get; init; }
}
