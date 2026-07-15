using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/media")]
public sealed class MediaController(Media mediaModel, Project projectModel, MediaFileStorageService storage, MediaReferenceService references) : ControllerBase
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

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IReadOnlyList<MediaDto>>> Upload([FromForm] List<IFormFile> files, [FromForm] uint? projectId, CancellationToken cancellationToken)
    {
        if (files.Count == 0) return Problem(statusCode: 400, title: "No files supplied", detail: "Sélectionnez au moins un fichier.");
        if (projectId.HasValue && await projectModel.GetByIdAsync(projectId.Value, cancellationToken) is null) return ProjectNotFound(projectId.Value);
        var created = new List<Media>();
        try
        {
            foreach (var file in files)
            {
                var stored = await storage.SaveAsync(file, cancellationToken);
                try
                {
                    created.Add(await mediaModel.CreateAsync(new Media { ProjectId = projectId, MediaType = stored.MediaType, OriginalFilename = stored.OriginalFilename, StoredFilename = stored.StoredFilename, FilePath = stored.FilePath, PublicUrl = stored.PublicUrl, MimeType = stored.MimeType, FileSize = stored.FileSize }, cancellationToken));
                }
                catch { await storage.DeleteAsync(stored.FilePath); throw; }
            }
            return StatusCode(StatusCodes.Status201Created, created.Select(Map).ToArray());
        }
        catch (InvalidDataException exception) { await RollbackUploadsAsync(created); return Problem(statusCode: 400, title: "Invalid media file", detail: exception.Message); }
        catch { await RollbackUploadsAsync(created); throw; }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken)
    {
        var media = await mediaModel.GetByIdAsync(id, cancellationToken);
        if (media is null) return MediaNotFound(id);
        if (await references.IsReferencedAsync(id, cancellationToken)) return Problem(statusCode: 409, title: "Media is in use", detail: "Ce média est référencé dans une documentation et ne peut pas être supprimé.");
        if (!await mediaModel.DeleteAsync(id, cancellationToken)) return MediaNotFound(id);
        await storage.DeleteAsync(media.FilePath);
        return NoContent();
    }

    private async Task RollbackUploadsAsync(IEnumerable<Media> items)
    {
        foreach (var item in items) { await mediaModel.DeleteAsync(item.Id, CancellationToken.None); await storage.DeleteAsync(item.FilePath); }
    }

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
