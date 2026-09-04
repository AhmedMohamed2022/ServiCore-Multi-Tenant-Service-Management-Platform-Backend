using ServiCore.Application.Categories.DTOs;
using ServiCore.Application.Common.Results;

namespace ServiCore.Application.Categories.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryDto>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> UpdateAsync(
        Guid categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeactivateAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);
}