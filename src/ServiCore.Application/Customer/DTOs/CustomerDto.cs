namespace ServiCore.Application.Customers.DTOs;

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);