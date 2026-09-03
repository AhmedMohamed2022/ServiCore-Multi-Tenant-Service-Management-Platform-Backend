namespace ServiCore.Application.Authentication.DTOs;

public record RegisterRequest(
    string Name,
    string Email,
    string Password,
    string OrganizationName);