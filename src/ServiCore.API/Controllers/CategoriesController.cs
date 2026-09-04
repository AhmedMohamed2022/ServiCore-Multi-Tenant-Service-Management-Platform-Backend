using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Categories.DTOs;
using ServiCore.Application.Categories.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageCategories")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _categoryService.CreateAsync(
                request,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { categoryId = result.Value!.Id },
            result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result =
            await _categoryService.GetAllAsync(
                cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("{categoryId:guid}")]
    public async Task<IActionResult> GetById(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result =
            await _categoryService.GetByIdAsync(
                categoryId,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Policy = "CanManageCategories")]
    public async Task<IActionResult> Update(
        Guid categoryId,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _categoryService.UpdateAsync(
                categoryId,
                request,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{categoryId:guid}")]
    [Authorize(Policy = "CanManageCategories")]
    public async Task<IActionResult> Deactivate(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result =
            await _categoryService.DeactivateAsync(
                categoryId,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new
            {
                error = result.Error
            });
        }

        return NoContent();
    }
}