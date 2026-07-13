using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/project-documents")]
public sealed class ProjectDocumentsController(
    IProjectDocumentService documentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectDocumentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDocumentDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await documentService.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    [ProducesResponseType<ProjectDocumentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDocumentDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        var document = await documentService.GetByIdAsync(id, cancellationToken);
        return document is null ? DocumentNotFound(id) : Ok(document);
    }

    [HttpPost]
    [ProducesResponseType<ProjectDocumentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDocumentDto>> Create(
        [FromBody] ProjectDocumentDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await documentService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }
        catch (AssociatedProjectNotFoundException exception)
        {
            return ProjectNotFound(exception.ProjectId);
        }
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
        try
        {
            var document = await documentService.UpdateAsync(id, request, cancellationToken);
            return document is null ? DocumentNotFound(id) : Ok(document);
        }
        catch (AssociatedProjectNotFoundException exception)
        {
            return ProjectNotFound(exception.ProjectId);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken) =>
        await documentService.DeleteAsync(id, cancellationToken) ? NoContent() : DocumentNotFound(id);

    private ObjectResult DocumentNotFound(uint id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Project document not found",
        detail: $"No project document with id {id} was found.");

    private ObjectResult ProjectNotFound(uint id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Project not found",
        detail: $"No project with id {id} was found.");
}
