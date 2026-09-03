using ServiCore.Application.Common.Results;

namespace ServiCore.Application.Common.Interfaces;

public interface ITenantResolver
{
    Task<Result<Guid>> ResolveAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}