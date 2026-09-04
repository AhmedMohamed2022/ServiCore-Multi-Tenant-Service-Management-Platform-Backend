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
}