using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Application.Customers.DTOs;
using ServiCore.Application.Customers.Interfaces;
using ServiCore.Domain.Entities;

namespace ServiCore.Application.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CustomerService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<CustomerDto>> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<CustomerDto>.Failure(
                "An organization context is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CustomerDto>.Failure(
                "Customer name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<CustomerDto>.Failure(
                "Customer email is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var emailExists =
            await _dbContext.CustomerEmailExistsAsync(
                organizationId,
                request.Email,
                cancellationToken: cancellationToken);

        if (emailExists)
        {
            return Result<CustomerDto>.Failure(
                "A customer with this email already exists.");
        }

        var customer = new Customer(
            organizationId,
            request.Name,
            request.Email,
            request.PhoneNumber);

        _dbContext.AddCustomer(customer);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<CustomerDto>.Success(
            MapCustomer(customer));
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<IReadOnlyList<CustomerDto>>.Failure(
                "An organization context is required.");
        }

        var customers =
            await _dbContext.GetCustomersAsync(
                _tenantContext.OrganizationId.Value,
                cancellationToken);

        var result = customers
            .Select(MapCustomer)
            .ToList();

        return Result<IReadOnlyList<CustomerDto>>
            .Success(result);
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customerResult =
            await GetActiveCustomerAsync(
                customerId,
                cancellationToken);

        if (!customerResult.IsSuccess)
        {
            return Result<CustomerDto>.Failure(
                customerResult.Error!);
        }

        return Result<CustomerDto>.Success(
            MapCustomer(customerResult.Value!));
    }

    public async Task<Result<CustomerDto>> UpdateAsync(
        Guid customerId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CustomerDto>.Failure(
                "Customer name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<CustomerDto>.Failure(
                "Customer email is required.");
        }

        var customerResult =
            await GetActiveCustomerAsync(
                customerId,
                cancellationToken);

        if (!customerResult.IsSuccess)
        {
            return Result<CustomerDto>.Failure(
                customerResult.Error!);
        }

        var customer = customerResult.Value!;

        var emailExists =
            await _dbContext.CustomerEmailExistsAsync(
                customer.OrganizationId,
                request.Email,
                customer.Id,
                cancellationToken);

        if (emailExists)
        {
            return Result<CustomerDto>.Failure(
                "A customer with this email already exists.");
        }

        customer.Update(
            request.Name,
            request.Email,
            request.PhoneNumber);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<CustomerDto>.Success(
            MapCustomer(customer));
    }

    public async Task<Result> DeactivateAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customerResult =
            await GetActiveCustomerAsync(
                customerId,
                cancellationToken);

        if (!customerResult.IsSuccess)
        {
            return Result.Failure(
                customerResult.Error!);
        }

        customerResult.Value!.Deactivate();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    private async Task<Result<Customer>> GetActiveCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<Customer>.Failure(
                "An organization context is required.");
        }

        var customer =
            await _dbContext.GetCustomerAsync(
                _tenantContext.OrganizationId.Value,
                customerId,
                activeOnly: true,
                cancellationToken);

        if (customer is null)
        {
            return Result<Customer>.Failure(
                "Customer not found.");
        }

        return Result<Customer>.Success(customer);
    }

    private static CustomerDto MapCustomer(Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.PhoneNumber,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt);
    }
}