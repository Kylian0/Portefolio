using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(
    IProjectService projectService,
    ILogger<ProjectsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectReadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<ProjectReadDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var projects = await projectService.GetAllAsync(cancellationToken);
            return Ok(projects);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while retrieving projects.");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to retrieve projects",
                detail: "An unexpected error occurred while processing the request.");
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ProjectReadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectReadDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectService.GetByIdAsync(id, cancellationToken);
            if (project is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Project not found",
                    detail: $"No project with id {id} was found.");
            }

            return Ok(project);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while retrieving project {ProjectId}.", id);
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to retrieve the project",
                detail: "An unexpected error occurred while processing the request.");
        }
    }

    [HttpPost]
    [ProducesResponseType<ProjectReadDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectReadDto>> Create(
        [FromBody] ProjectCreateDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }
        catch (ProjectSlugConflictException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Project slug already exists",
                detail: "Another project already uses this slug.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while creating a project.");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to create the project",
                detail: "An unexpected error occurred while processing the request.");
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType<ProjectReadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectReadDto>> Update(
        uint id,
        [FromBody] ProjectUpdateDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectService.UpdateAsync(id, request, cancellationToken);
            if (project is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Project not found",
                    detail: $"No project with id {id} was found.");
            }

            return Ok(project);
        }
        catch (ProjectSlugConflictException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Project slug already exists",
                detail: "Another project already uses this slug.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while updating project {ProjectId}.", id);
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to update the project",
                detail: "An unexpected error occurred while processing the request.");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await projectService.DeleteAsync(id, cancellationToken))
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Project not found",
                    detail: $"No project with id {id} was found.");
            }

            return NoContent();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while deleting project {ProjectId}.", id);
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to delete the project",
                detail: "An unexpected error occurred while processing the request.");
        }
    }
}
