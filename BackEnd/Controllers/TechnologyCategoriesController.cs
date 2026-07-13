using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/technology-categories")]
public sealed class TechnologyCategoriesController(TechnologyCategory categoryModel) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TechnologyCategoryDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await categoryModel.GetAllAsync(cancellationToken)).Select(Map).ToArray());

    [HttpGet("{id}")]
    public async Task<ActionResult<TechnologyCategoryDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        var category = await categoryModel.GetByIdAsync(id, cancellationToken);
        return category is null ? CategoryNotFound(id) : Ok(Map(category));
    }

    [HttpPost]
    public async Task<ActionResult<TechnologyCategoryDto>> Create([FromBody] TechnologyCategoryDto request, CancellationToken cancellationToken)
    {
        try
        {
            var category = Map(await categoryModel.CreateAsync(ToEntity(request), cancellationToken));
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (TechnologyCategoryConflictException)
        {
            return CategoryConflict();
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TechnologyCategoryDto>> Update(uint id, [FromBody] TechnologyCategoryDto request, CancellationToken cancellationToken)
    {
        if (await categoryModel.GetByIdAsync(id, cancellationToken) is null)
        {
            return CategoryNotFound(id);
        }

        try
        {
            var category = await categoryModel.UpdateAsync(id, ToEntity(request, id), cancellationToken);
            return category is null ? CategoryNotFound(id) : Ok(Map(category));
        }
        catch (TechnologyCategoryConflictException)
        {
            return CategoryConflict();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken)
    {
        try
        {
            return await categoryModel.DeleteAsync(id, cancellationToken) ? NoContent() : CategoryNotFound(id);
        }
        catch (TechnologyCategoryInUseException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Technology category is in use",
                detail: "This category cannot be deleted because one or more technologies still reference it.");
        }
    }

    private static TechnologyCategory ToEntity(TechnologyCategoryDto category, uint id = 0) => new()
    {
        Id = id,
        Name = category.Name.Trim(),
        Slug = category.Slug.Trim(),
        DisplayOrder = category.DisplayOrder
    };

    private static TechnologyCategoryDto Map(TechnologyCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        DisplayOrder = category.DisplayOrder,
        CreatedAt = category.CreatedAt
    };

    private ObjectResult CategoryNotFound(uint id) => Problem(statusCode: 404, title: "Technology category not found", detail: $"No technology category with id {id} was found.");
    private ObjectResult CategoryConflict() => Problem(statusCode: 409, title: "Technology category already exists", detail: "Another technology category already uses this name or slug.");
}
