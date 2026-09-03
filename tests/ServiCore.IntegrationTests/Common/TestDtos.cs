namespace ServiCore.IntegrationTests.Common;

public record RegisterTestResponse(
    Guid UserId,
    Guid OrganizationId,
    string OrganizationName);

public record LoginTestResponse(
    string Token);

public record TeamTestResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    DateTime CreatedAt);
public sealed record OrganizationTestResponse(
    Guid UserId,
    Guid OrganizationId,
    string OrganizationName);
