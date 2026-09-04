using ServiCore.Application.Categories.DTOs;
using ServiCore.Application.Categories.Interfaces;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Domain.Entities;

namespace ServiCore.Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CategoryService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<CategoryDto>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<CategoryDto>.Failure(
                "An organization context is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CategoryDto>.Failure(
                "Category name is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var exists =
            await _dbContext.CategoryNameExistsAsync(
                organizationId,
                request.Name,
                cancellationToken: cancellationToken);

        if (exists)
        {
            return Result<CategoryDto>.Failure(
                "A category with this name already exists.");
        }

        var category = new Category(
            organizationId,
            request.Name,
            request.Description);

        _dbContext.AddCategory(category);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<CategoryDto>.Success(
            MapCategory(category));
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<IReadOnlyList<CategoryDto>>.Failure(
                "An organization context is required.");
        }

        var categories =
            await _dbContext.GetCategoriesAsync(
                _tenantContext.OrganizationId.Value,
                cancellationToken);

        var result = categories
            .Select(MapCategory)
            .ToList();

        return Result<IReadOnlyList<CategoryDto>>
            .Success(result);
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var categoryResult =
            await GetActiveCategoryAsync(
                categoryId,
                cancellationToken);

        if (!categoryResult.IsSuccess)
        {
            return Result<CategoryDto>.Failure(
                categoryResult.Error!);
        }

        return Result<CategoryDto>.Success(
            MapCategory(categoryResult.Value!));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(
        Guid categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CategoryDto>.Failure(
                "Category name is required.");
        }

        var categoryResult =
            await GetActiveCategoryAsync(
                categoryId,
                cancellationToken);

        if (!categoryResult.IsSuccess)
        {
            return Result<CategoryDto>.Failure(
                categoryResult.Error!);
        }

        var category = categoryResult.Value!;

        var exists =
            await _dbContext.CategoryNameExistsAsync(
                category.OrganizationId,
                request.Name,
                category.Id,
                cancellationToken);

        if (exists)
        {
            return Result<CategoryDto>.Failure(
                "A category with this name already exists.");
        }

        category.Update(
            request.Name,
            request.Description);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<CategoryDto>.Success(
            MapCategory(category));
    }

    public async Task<Result> DeactivateAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var categoryResult =
            await GetActiveCategoryAsync(
                categoryId,
                cancellationToken);

        if (!categoryResult.IsSuccess)
        {
            return Result.Failure(
                categoryResult.Error!);
        }

        categoryResult.Value!.Deactivate();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    private async Task<Result<Category>> GetActiveCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<Category>.Failure(
                "An organization context is required.");
        }

        var category =
            await _dbContext.GetCategoryAsync(
                _tenantContext.OrganizationId.Value,
                categoryId,
                activeOnly: true,
                cancellationToken);

        if (category is null)
        {
            return Result<Category>.Failure(
                "Category not found.");
        }

        return Result<Category>.Success(category);
    }

    private static CategoryDto MapCategory(
        Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAt,
            category.UpdatedAt);
    }
}