using BackEnd.Dtos;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/project-learnings")]
public sealed class ProjectLearningsController(ProjectLearning learningModel, Project projectModel) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectLearningDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await learningModel.GetAllAsync(cancellationToken)).Select(Map).ToArray());

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectLearningDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        var learning = await learningModel.GetByIdAsync(id, cancellationToken);
        return learning is null ? LearningNotFound(id) : Ok(Map(learning));
    }

    [HttpGet("/api/projects/{projectId}/learnings")]
    public async Task<ActionResult<IReadOnlyList<ProjectLearningDto>>> GetByProjectId(uint projectId, CancellationToken cancellationToken)
    {
        if (await projectModel.GetByIdAsync(projectId, cancellationToken) is null) return ProjectNotFound(projectId);
        return Ok((await learningModel.GetByProjectIdAsync(projectId, cancellationToken)).Select(Map).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<ProjectLearningDto>> Create([FromBody] ProjectLearningDto request, CancellationToken cancellationToken)
    {
        if (!ValidateContent(request.Content)) return ValidationProblem(ModelState);
        if (await projectModel.GetByIdAsync(request.ProjectId, cancellationToken) is null) return ProjectNotFound(request.ProjectId);
        var learning = Map(await learningModel.CreateAsync(ToEntity(request), cancellationToken));
        return CreatedAtAction(nameof(GetById), new { id = learning.Id }, learning);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectLearningDto>> Update(uint id, [FromBody] ProjectLearningDto request, CancellationToken cancellationToken)
    {
        if (!ValidateContent(request.Content)) return ValidationProblem(ModelState);
        if (await learningModel.GetByIdAsync(id, cancellationToken) is null) return LearningNotFound(id);
        if (await projectModel.GetByIdAsync(request.ProjectId, cancellationToken) is null) return ProjectNotFound(request.ProjectId);
        var learning = await learningModel.UpdateAsync(id, ToEntity(request, id), cancellationToken);
        return learning is null ? LearningNotFound(id) : Ok(Map(learning));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken) =>
        await learningModel.DeleteAsync(id, cancellationToken) ? NoContent() : LearningNotFound(id);

    private bool ValidateContent(string content)
    {
        if (!string.IsNullOrWhiteSpace(content)) return true;
        ModelState.AddModelError(nameof(ProjectLearningDto.Content), "Content cannot be empty or contain only whitespace.");
        return false;
    }

    private static ProjectLearning ToEntity(ProjectLearningDto learning, uint id = 0) => new()
    {
        Id = id,
        ProjectId = learning.ProjectId,
        ProjectTitle = string.Empty,
        Content = learning.Content.Trim(),
        DisplayOrder = learning.DisplayOrder
    };

    private static ProjectLearningDto Map(ProjectLearning learning) => new()
    {
        Id = learning.Id,
        ProjectId = learning.ProjectId,
        ProjectTitle = learning.ProjectTitle,
        Content = learning.Content,
        DisplayOrder = learning.DisplayOrder,
        CreatedAt = learning.CreatedAt
    };

    private ObjectResult LearningNotFound(uint id) => Problem(statusCode: 404, title: "Project learning not found", detail: $"No project learning with id {id} was found.");
    private ObjectResult ProjectNotFound(uint id) => Problem(statusCode: 404, title: "Project not found", detail: $"No project with id {id} was found.");
}
