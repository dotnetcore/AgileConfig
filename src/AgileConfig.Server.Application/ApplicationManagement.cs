using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Application;

public interface IApplicationManagementService
{
    Task<IReadOnlyList<ApplicationDetails>> GetAllAsync();

    Task<ApplicationDetails> GetAsync(string applicationId);

    Task<ApplicationResult<App>> CreateAsync(CreateApplicationCommand command);

    Task<ApplicationResult<App>> UpdateAsync(UpdateApplicationCommand command);

    Task<ApplicationResult<App>> DeleteAsync(DeleteApplicationCommand command);
}

public sealed record ApplicationDetails(App Application, IReadOnlyList<string> InheritedApplicationIds);

public sealed record CreateApplicationCommand(
    string Id,
    string Name,
    string Group,
    string Secret,
    bool Enabled,
    bool IsInheritanceSource,
    IReadOnlyList<string> InheritsFrom,
    bool ValidateInheritanceReferences = false);

public sealed record UpdateApplicationCommand(
    string Id,
    string Name,
    string Group,
    string Secret,
    bool Enabled,
    bool IsInheritanceSource,
    IReadOnlyList<string> InheritsFrom,
    bool ValidateInheritanceReferences = false);

public sealed record DeleteApplicationCommand(string Id);
