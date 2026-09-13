using ServiCore.Application.Common.Results;
using ServiCore.Application.Customers.DTOs;

namespace ServiCore.Application.Customers.Interfaces;

public interface ICustomerService
{
    Task<Result<CustomerDto>> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CustomerDto>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Result<CustomerDto>> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerDto>> UpdateAsync(
        Guid customerId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeactivateAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    // Not tenant-scoped: this is how a customer learns which
    // organization(s) they belong to in the first place, so it must be
    // callable before any X-Organization-Id context exists. Mirrors
    // IOrganizationService.GetMineAsync for staff.
    Task<Result<IReadOnlyList<CustomerMembershipDto>>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}