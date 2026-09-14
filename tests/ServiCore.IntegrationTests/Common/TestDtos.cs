namespace ServiCore.IntegrationTests.Common;

public sealed record RegisterTestResponse(
    Guid UserId,
    Guid OrganizationId,
    string OrganizationName);

public sealed record LoginTestResponse(string Token);

public sealed record TeamTestResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    DateTime CreatedAt);

public sealed record OrganizationTestResponse(
    Guid UserId,
    Guid OrganizationId,
    string OrganizationName);

public sealed record CustomerTestResponse(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CategoryTestResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record TicketTestResponse(
    Guid Id,
    Guid OrganizationId,
    Guid CustomerId,
    Guid TeamId,
    Guid? AssignedAgentId,
    Guid CategoryId,
    string Title,
    string Description,
    int Priority,
    int Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt);
