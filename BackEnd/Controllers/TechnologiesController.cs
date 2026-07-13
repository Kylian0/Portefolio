using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/technologies")]
public sealed class TechnologiesController(
    Technology technologyModel,
    TechnologyCategory categoryModel) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TechnologyDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await technologyModel.GetAllAsync(cancellationToken)).Select(Map).ToArray());

    [HttpGet("{id}")]
    public async Task<ActionResult<TechnologyDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        var technology = await technologyModel.GetByIdAsync(id, cancellationToken);
        return technology is null ? TechnologyNotFound(id) : Ok(Map(technology));
    }

    [HttpGet("/api/technology-categories/{categoryId}/technologies")]
    public async Task<ActionResult<IReadOnlyList<TechnologyDto>>> GetByCategoryId(uint categoryId, CancellationToken cancellationToken)
    {
        if (await categoryModel.GetByIdAsync(categoryId, cancellationToken) is null)
        {
            return CategoryNotFound(categoryId);
        }

        return Ok((await technologyModel.GetByCategoryIdAsync(categoryId, cancellationToken)).Select(Map).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<TechnologyDto>> Create([FromBody] TechnologyDto request, CancellationToken cancellationToken)
    {
        if (await categoryModel.GetByIdAsync(request.CategoryId, cancellationToken) is null)
        {
            return CategoryNotFound(request.CategoryId);
        }

        try
        {
            var technology = Map(await technologyModel.CreateAsync(ToEntity(request), cancellationToken));
            return CreatedAtAction(nameof(GetById), new { id = technology.Id }, technology);
        }
        catch (TechnologyConflictException)
        {
            return TechnologyConflict();
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TechnologyDto>> Update(uint id, [FromBody] TechnologyDto request, CancellationToken cancellationToken)
    {
        if (await technologyModel.GetByIdAsync(id, cancellationToken) is null)
        {
            return TechnologyNotFound(id);
        }
        if (await categoryModel.GetByIdAsync(request.CategoryId, cancellationToken) is null)
        {
            return CategoryNotFound(request.CategoryId);
        }

        try
        {
            var technology = await technologyModel.UpdateAsync(id, ToEntity(request, id), cancellationToken);
            return technology is null ? TechnologyNotFound(id) : Ok(Map(technology));
        }
        catch (TechnologyConflictException)
        {
            return TechnologyConflict();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken)
    {
        try
        {
            return await technologyModel.DeleteAsync(id, cancellationToken) ? NoContent() : TechnologyNotFound(id);
        }
        catch (TechnologyInUseException)
        {
            return Problem(statusCode: 409, title: "Technology is in use", detail: "This technology cannot be deleted because one or more projects still reference it.");
        }
    }

    private static Technology ToEntity(TechnologyDto technology, uint id = 0) => new()
    {
        Id = id,
        CategoryId = technology.CategoryId,
        CategoryName = string.Empty,
        CategorySlug = string.Empty,
        Name = technology.Name.Trim(),
        Slug = technology.Slug.Trim(),
        IconUrl = string.IsNullOrWhiteSpace(technology.IconUrl) ? null : technology.IconUrl.Trim(),
        OfficialUrl = string.IsNullOrWhiteSpace(technology.OfficialUrl) ? null : technology.OfficialUrl.Trim()
    };

    private static TechnologyDto Map(Technology technology) => new()
    {
        Id = technology.Id,
        CategoryId = technology.CategoryId,
        CategoryName = technology.CategoryName,
        CategorySlug = technology.CategorySlug,
        Name = technology.Name,
        Slug = technology.Slug,
        IconUrl = technology.IconUrl,
        OfficialUrl = technology.OfficialUrl,
        CreatedAt = technology.CreatedAt,
        UpdatedAt = technology.UpdatedAt
    };

    private ObjectResult TechnologyNotFound(uint id) => Problem(statusCode: 404, title: "Technology not found", detail: $"No technology with id {id} was found.");
    private ObjectResult CategoryNotFound(uint id) => Problem(statusCode: 404, title: "Technology category not found", detail: $"No technology category with id {id} was found.");
    private ObjectResult TechnologyConflict() => Problem(statusCode: 409, title: "Technology already exists", detail: "Another technology already uses this name or slug.");
}
