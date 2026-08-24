using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgileConfig.Server.Apisite.Controllers.api.v2.Models;

public sealed class ApplicationResource
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Group { get; init; }
    public bool Enabled { get; init; }
    public bool IsInheritanceSource { get; init; }
    public IReadOnlyList<string> InheritsFrom { get; init; } = [];
    public string Creator { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class CreateApplicationRequest
{
    [Required, MaxLength(36)]
    public string Id { get; init; }

    [Required, MaxLength(50)]
    public string Name { get; init; }

    [MaxLength(50)]
    public string Group { get; init; }

    [MaxLength(36)]
    public string Secret { get; init; }

    public bool Enabled { get; init; } = true;
    public bool IsInheritanceSource { get; init; }
    public List<string> InheritsFrom { get; init; } = [];
}

public sealed class UpdateApplicationRequest
{
    [Required, MaxLength(50)]
    public string Name { get; init; }

    [MaxLength(50)]
    public string Group { get; init; }

    [MaxLength(36)]
    public string Secret { get; init; }

    public bool Enabled { get; init; }
    public bool IsInheritanceSource { get; init; }
    public List<string> InheritsFrom { get; init; } = [];
}
