namespace ServiCore.Application.Authentication.DTOs;

public record RegisterResponse(
    Guid UserId,
    Guid OrganizationId,
    string OrganizationName);