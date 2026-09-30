using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Application.Customers.DTOs;
using ServiCore.Application.Customers.Interfaces;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.Application.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public CustomerService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
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
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<CustomerDto>.Failure(
                "An organization context is required.");
        }

        if (!_currentUser.UserId.HasValue)
        {
            return Result<CustomerDto>.Failure(
                "An authenticated user is required.");
        }

        var organizationId = _tenantContext.OrganizationId.Value;
        var userId = _currentUser.UserId.Value;

        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            userId,
            cancellationToken);

        // OrganizationRole applies only to organization staff.
        // Customers are represented by a Customer record linked to the
        // authenticated Identity user; they are not an organization role.
        var isStaff =
            role == OrganizationRole.Owner ||
            role == OrganizationRole.Manager ||
            role == OrganizationRole.Agent;

        if (!isStaff)
        {
            var belongsToCustomer =
                await _dbContext.CustomerBelongsToUserAsync(
                    organizationId,
                    customerId,
                    userId,
                    cancellationToken);

            if (!belongsToCustomer)
            {
                // Deliberately use the same result as a missing customer so a
                // customer cannot probe whether another customer exists.
                return Result<CustomerDto>.Failure(
                    "Customer not found.");
            }
        }

        var customer = await _dbContext.GetCustomerAsync(
            organizationId,
            customerId,
            activeOnly: true,
            cancellationToken);

        if (customer is null)
        {
            return Result<CustomerDto>.Failure(
                "Customer not found.");
        }

        return Result<CustomerDto>.Success(
            MapCustomer(customer));
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

    public async Task<Result<IReadOnlyList<CustomerMembershipDto>>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var memberships =
            await _dbContext.GetCustomerMembershipsForUserAsync(
                userId,
                cancellationToken);

        var result = memberships
            .Select(m => new CustomerMembershipDto(
                m.CustomerId,
                m.OrganizationId,
                m.OrganizationName))
            .ToList() as IReadOnlyList<CustomerMembershipDto>;

        return Result<IReadOnlyList<CustomerMembershipDto>>.Success(result);
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