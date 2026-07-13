using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/project-technologies")]
public sealed class ProjectTechnologiesController(
    ProjectTechnology relationModel,
    Project projectModel,
    Technology technologyModel) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectTechnologyDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await relationModel.GetAllAsync(cancellationToken)).Select(Map).ToArray());

    [HttpGet("/api/projects/{projectId}/technologies")]
    public async Task<ActionResult<IReadOnlyList<ProjectTechnologyDto>>> GetByProjectId(uint projectId, CancellationToken cancellationToken)
    {
        if (await projectModel.GetByIdAsync(projectId, cancellationToken) is null) return ProjectNotFound(projectId);
        return Ok((await relationModel.GetByProjectIdAsync(projectId, cancellationToken)).Select(Map).ToArray());
    }

    [HttpGet("/api/technologies/{technologyId}/projects")]
    public async Task<ActionResult<IReadOnlyList<ProjectTechnologyDto>>> GetByTechnologyId(uint technologyId, CancellationToken cancellationToken)
    {
        if (await technologyModel.GetByIdAsync(technologyId, cancellationToken) is null) return TechnologyNotFound(technologyId);
        return Ok((await relationModel.GetByTechnologyIdAsync(technologyId, cancellationToken)).Select(Map).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<ProjectTechnologyDto>> Create([FromBody] ProjectTechnologyDto request, CancellationToken cancellationToken)
    {
        if (request.ProjectId == 0) ModelState.AddModelError(nameof(request.ProjectId), "ProjectId must be greater than 0.");
        if (request.TechnologyId == 0) ModelState.AddModelError(nameof(request.TechnologyId), "TechnologyId must be greater than 0.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (await projectModel.GetByIdAsync(request.ProjectId, cancellationToken) is null) return ProjectNotFound(request.ProjectId);
        if (await technologyModel.GetByIdAsync(request.TechnologyId, cancellationToken) is null) return TechnologyNotFound(request.TechnologyId);

        try
        {
            var relation = Map(await relationModel.CreateAsync(ToEntity(request), cancellationToken));
            return StatusCode(StatusCodes.Status201Created, relation);
        }
        catch (ProjectTechnologyConflictException)
        {
            return Problem(statusCode: 409, title: "Project technology relation already exists", detail: "This project is already associated with this technology.");
        }
    }

    [HttpPut("/api/projects/{projectId}/technologies/{technologyId}")]
    public async Task<ActionResult<ProjectTechnologyDto>> Update(
        uint projectId,
        uint technologyId,
        [FromBody] ProjectTechnologyDto request,
        CancellationToken cancellationToken)
    {
        if (await relationModel.GetByIdsAsync(projectId, technologyId, cancellationToken) is null) return RelationNotFound(projectId, technologyId);
        var relation = await relationModel.UpdateAsync(ToEntity(request, projectId, technologyId), cancellationToken);
        return relation is null ? RelationNotFound(projectId, technologyId) : Ok(Map(relation));
    }

    [HttpDelete("/api/projects/{projectId}/technologies/{technologyId}")]
    public async Task<IActionResult> Delete(uint projectId, uint technologyId, CancellationToken cancellationToken) =>
        await relationModel.DeleteAsync(projectId, technologyId, cancellationToken)
            ? NoContent()
            : RelationNotFound(projectId, technologyId);

    private static ProjectTechnology ToEntity(ProjectTechnologyDto relation, uint? projectId = null, uint? technologyId = null) => new()
    {
        ProjectId = projectId ?? relation.ProjectId,
        ProjectTitle = string.Empty,
        ProjectSlug = string.Empty,
        TechnologyId = technologyId ?? relation.TechnologyId,
        TechnologyName = string.Empty,
        TechnologySlug = string.Empty,
        CategoryId = 0,
        CategoryName = string.Empty,
        IsPrimary = relation.IsPrimary,
        DisplayOrder = relation.DisplayOrder
    };

    private static ProjectTechnologyDto Map(ProjectTechnology relation) => new()
    {
        ProjectId = relation.ProjectId,
        ProjectTitle = relation.ProjectTitle,
        ProjectSlug = relation.ProjectSlug,
        TechnologyId = relation.TechnologyId,
        TechnologyName = relation.TechnologyName,
        TechnologySlug = relation.TechnologySlug,
        CategoryId = relation.CategoryId,
        CategoryName = relation.CategoryName,
        IsPrimary = relation.IsPrimary,
        DisplayOrder = relation.DisplayOrder
    };

    private ObjectResult ProjectNotFound(uint id) => Problem(statusCode: 404, title: "Project not found", detail: $"No project with id {id} was found.");
    private ObjectResult TechnologyNotFound(uint id) => Problem(statusCode: 404, title: "Technology not found", detail: $"No technology with id {id} was found.");
    private ObjectResult RelationNotFound(uint projectId, uint technologyId) => Problem(statusCode: 404, title: "Project technology relation not found", detail: $"Project {projectId} is not associated with technology {technologyId}.");
}
