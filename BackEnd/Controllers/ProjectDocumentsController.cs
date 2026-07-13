using BackEnd.Dtos;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/project-documents")]
public sealed class ProjectDocumentsController(
    ProjectDocument documentModel,
    Project projectModel) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectDocumentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDocumentDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await documentModel.GetAllAsync(cancellationToken)).Select(Map).ToArray());

    [HttpGet("{id}")]
    [ProducesResponseType<ProjectDocumentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDocumentDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        var document = await documentModel.GetByIdAsync(id, cancellationToken);
        return document is null ? DocumentNotFound(id) : Ok(Map(document));
    }

    [HttpPost]
    [ProducesResponseType<ProjectDocumentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDocumentDto>> Create(
        [FromBody] ProjectDocumentDto request,
        CancellationToken cancellationToken)
    {
        if (await projectModel.GetByIdAsync(request.ProjectId, cancellationToken) is null)
        {
            return ProjectNotFound(request.ProjectId);
        }

        var document = Map(await documentModel.CreateAsync(ToEntity(request), cancellationToken));
        return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<ProjectDocumentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDocumentDto>> Update(
        uint id,
        [FromBody] ProjectDocumentDto request,
        CancellationToken cancellationToken)
    {
        if (await documentModel.GetByIdAsync(id, cancellationToken) is null)
        {
            return DocumentNotFound(id);
        }
        if (await projectModel.GetByIdAsync(request.ProjectId, cancellationToken) is null)
        {
            return ProjectNotFound(request.ProjectId);
        }

        var document = await documentModel.UpdateAsync(id, ToEntity(request, id), cancellationToken);
        return document is null ? DocumentNotFound(id) : Ok(Map(document));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken) =>
        await documentModel.DeleteAsync(id, cancellationToken) ? NoContent() : DocumentNotFound(id);

    [HttpGet("/api/projects/{projectId}/documents")]
    public async Task<ActionResult<IReadOnlyList<ProjectDocumentDto>>> GetByProjectId(
        uint projectId,
        CancellationToken cancellationToken)
    {
        if (await projectModel.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return ProjectNotFound(projectId);
        }

        var documents = await documentModel.GetByProjectIdAsync(projectId, cancellationToken);
        return Ok(documents.Select(Map).ToArray());
    }

    private static ProjectDocument ToEntity(ProjectDocumentDto document, uint id = 0) => new()
    {
        Id = id,
        ProjectId = document.ProjectId,
        Title = document.Title.Trim()
    };

    internal static ProjectDocumentDto Map(ProjectDocument document) => new()
    {
        Id = document.Id,
        ProjectId = document.ProjectId,
        Title = document.Title,
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };

    private ObjectResult DocumentNotFound(uint id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Project document not found",
        detail: $"No project document with id {id} was found.");

    private ObjectResult ProjectNotFound(uint id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Project not found",
        detail: $"No project with id {id} was found.");
}
