using BackEnd.Dtos;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/project-document-blocks")]
public sealed class ProjectDocumentBlocksController(
    ProjectDocumentBlock blockModel,
    ProjectDocument documentModel,
    HtmlDocumentBlockConverter converter) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDocumentBlockDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await blockModel.GetAllAsync(cancellationToken)).Select(converter.SanitizeBlock).Select(Map).ToArray());

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDocumentBlockDto>> GetById(uint id, CancellationToken cancellationToken)
    {
        var block = await blockModel.GetByIdAsync(id, cancellationToken);
        return block is null ? BlockNotFound(id) : Ok(Map(converter.SanitizeBlock(block)));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDocumentBlockDto>> Create([FromBody] ProjectDocumentBlockDto request, CancellationToken cancellationToken)
    {
        if (await documentModel.GetByIdAsync(request.DocumentId, cancellationToken) is null)
        {
            return DocumentNotFound(request.DocumentId);
        }

        var block = Map(await blockModel.CreateAsync(converter.SanitizeBlock(ToEntity(request)), cancellationToken));
        return CreatedAtAction(nameof(GetById), new { id = block.Id }, block);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDocumentBlockDto>> Update(uint id, [FromBody] ProjectDocumentBlockDto request, CancellationToken cancellationToken)
    {
        if (await blockModel.GetByIdAsync(id, cancellationToken) is null)
        {
            return BlockNotFound(id);
        }
        if (await documentModel.GetByIdAsync(request.DocumentId, cancellationToken) is null)
        {
            return DocumentNotFound(request.DocumentId);
        }

        var block = await blockModel.UpdateAsync(id, converter.SanitizeBlock(ToEntity(request, id)), cancellationToken);
        return block is null ? BlockNotFound(id) : Ok(Map(converter.SanitizeBlock(block)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(uint id, CancellationToken cancellationToken) =>
        await blockModel.DeleteAsync(id, cancellationToken) ? NoContent() : BlockNotFound(id);

    [HttpGet("/api/project-documents/{documentId}/blocks")]
    public async Task<ActionResult<IReadOnlyList<ProjectDocumentBlockDto>>> GetByDocumentId(
        uint documentId,
        CancellationToken cancellationToken)
    {
        if (await documentModel.GetByIdAsync(documentId, cancellationToken) is null)
        {
            return DocumentNotFound(documentId);
        }

        var blocks = await blockModel.GetByDocumentIdAsync(documentId, cancellationToken);
        return Ok(blocks.Select(converter.SanitizeBlock).Select(Map).ToArray());
    }

    [HttpPut("/api/project-documents/{documentId}/content")]
    public async Task<ActionResult<IReadOnlyList<ProjectDocumentBlockDto>>> Synchronize(
        uint documentId,
        [FromBody] ProjectDocumentSyncDto request,
        CancellationToken cancellationToken)
    {
        if (await documentModel.GetByIdAsync(documentId, cancellationToken) is null) return DocumentNotFound(documentId);
        var normalized = converter.Convert(documentId, request.Html);
        var blocks = await blockModel.SynchronizeAsync(documentId, request.Title, normalized, cancellationToken);
        return Ok(blocks.Select(Map).ToArray());
    }

    private static ProjectDocumentBlock ToEntity(ProjectDocumentBlockDto block, uint id = 0) => new()
    {
        Id = id,
        DocumentId = block.DocumentId,
        BlockType = block.BlockType,
        Content = block.Content,
        Settings = block.Settings,
        DisplayOrder = block.DisplayOrder
    };

    internal static ProjectDocumentBlockDto Map(ProjectDocumentBlock block) => new()
    {
        Id = block.Id,
        DocumentId = block.DocumentId,
        BlockType = block.BlockType,
        Content = block.Content,
        Settings = block.Settings,
        DisplayOrder = block.DisplayOrder,
        CreatedAt = block.CreatedAt,
        UpdatedAt = block.UpdatedAt
    };

    private ObjectResult BlockNotFound(uint id) => Problem(statusCode: 404, title: "Project document block not found", detail: $"No project document block with id {id} was found.");
    private ObjectResult DocumentNotFound(uint id) => Problem(statusCode: 404, title: "Project document not found", detail: $"No project document with id {id} was found.");
}
