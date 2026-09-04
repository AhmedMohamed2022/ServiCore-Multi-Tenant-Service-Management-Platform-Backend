namespace ServiCore.Application.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);