using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/projects/{projectId}/documents")]
public sealed class ProjectDocumentsByProjectController(IProjectDocumentService documentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectDocumentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectDocumentDto>>> GetByProjectId(
        uint projectId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await documentService.GetByProjectIdAsync(projectId, cancellationToken));
        }
        catch (AssociatedProjectNotFoundException)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Project not found",
                detail: $"No project with id {projectId} was found.");
        }
    }
}
