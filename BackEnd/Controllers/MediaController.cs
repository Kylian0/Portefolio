using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/media")]
public sealed class MediaController(Media mediaModel, Project projectModel) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MediaDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await mediaModel.GetAllAsync(cancellationToken)).Select(Map).ToArray());

    [HttpGet("{id}")]
    public async Task<ActionResult<MediaDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        var media = await mediaModel.GetByIdAsync(id, cancellationToken);
        return media is null ? MediaNotFound(id) : Ok(Map(media));
    }

    [HttpGet("/api/projects/{projectId}/media")]
    public async Task<ActionResult<IReadOnlyList<MediaDto>>> GetByProjectId(uint projectId, CancellationToken cancellationToken)
    {
        if (await projectModel.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return ProjectNotFound(projectId);
        }

        return Ok((await mediaModel.GetByProjectIdAsync(projectId, cancellationToken)).Select(Map).ToArray());
    }

    [HttpPost]
    public async Task<ActionResult<MediaDto>> Create([FromBody] MediaDto request, CancellationToken cancellationToken)
    {
        if (request.ProjectId.HasValue && await projectModel.GetByIdAsync(request.ProjectId.Value, cancellationToken) is null)
        {
            return ProjectNotFound(request.ProjectId.Value);
        }

        try
        {
            var media = Map(await mediaModel.CreateAsync(ToEntity(request), cancellationToken));
            return CreatedAtAction(nameof(GetById), new { id = media.Id }, media);
        }
        catch (MediaStoredFilenameConflictException)
        {
            return FilenameConflict();
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MediaDto>> Update(uint id, [FromBody] MediaDto request, CancellationToken cancellationToken)
    {
        if (await mediaModel.GetByIdAsync(id, cancellationToken) is null)
        {
            return MediaNotFound(id);
        }
        if (request.ProjectId.HasValue && await projectModel.GetByIdAsync(request.ProjectId.Value, cancellationToken) is null)
        {
            return ProjectNotFound(request.ProjectId.Value);
        }

        try
        {
            var media = await mediaModel.UpdateAsync(id, ToEntity(request, id), cancellationToken);
            return media is null ? MediaNotFound(id) : Ok(Map(media));
        }
        catch (MediaStoredFilenameConflictException)
        {
            return FilenameConflict();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken) =>
        await mediaModel.DeleteAsync(id, cancellationToken) ? NoContent() : MediaNotFound(id);

    private static Media ToEntity(MediaDto media, uint id = 0) => new()
    {
        Id = id,
        ProjectId = media.ProjectId,
        MediaType = media.MediaType,
        OriginalFilename = media.OriginalFilename.Trim(),
        StoredFilename = media.StoredFilename.Trim(),
        FilePath = media.FilePath.Trim(),
        PublicUrl = media.PublicUrl,
        MimeType = media.MimeType.Trim(),
        FileSize = media.FileSize,
        AltText = media.AltText,
        Caption = media.Caption
    };

    private static MediaDto Map(Media media) => new()
    {
        Id = media.Id,
        ProjectId = media.ProjectId,
        MediaType = media.MediaType,
        OriginalFilename = media.OriginalFilename,
        StoredFilename = media.StoredFilename,
        FilePath = media.FilePath,
        PublicUrl = media.PublicUrl,
        MimeType = media.MimeType,
        FileSize = media.FileSize,
        AltText = media.AltText,
        Caption = media.Caption,
        CreatedAt = media.CreatedAt
    };

    private ObjectResult MediaNotFound(uint id) => Problem(statusCode: 404, title: "Media not found", detail: $"No media item with id {id} was found.");
    private ObjectResult ProjectNotFound(uint id) => Problem(statusCode: 404, title: "Project not found", detail: $"No project with id {id} was found.");
    private ObjectResult FilenameConflict() => Problem(statusCode: 409, title: "Stored filename already exists", detail: "Another media item already uses this stored filename.");
}
