namespace ServiCore.Application.Categories.DTOs;

public record UpdateCategoryRequest(
    string Name,
    string? Description);