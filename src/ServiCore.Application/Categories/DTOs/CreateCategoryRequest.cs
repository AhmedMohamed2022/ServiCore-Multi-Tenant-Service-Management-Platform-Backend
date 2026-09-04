namespace ServiCore.Application.Categories.DTOs;

public record CreateCategoryRequest(
    string Name,
    string? Description);