using BackEnd.Dtos;
using BackEnd.Exceptions;
using BackEnd.Models;
using BackEnd.Repositories;

namespace BackEnd.Services;

public sealed class ProjectDocumentService(
    IProjectDocumentRepository documentRepository,
    IProjectRepository projectRepository) : IProjectDocumentService
{
    public async Task<IReadOnlyList<ProjectDocumentDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await documentRepository.GetAllAsync(cancellationToken)).Select(MapToDto).ToArray();

    public async Task<ProjectDocumentDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(id, cancellationToken);
        return document is null ? null : MapToDto(document);
    }

    public async Task<IReadOnlyList<ProjectDocumentDto>> GetByProjectIdAsync(
        uint projectId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProjectExistsAsync(projectId, cancellationToken);
        return (await documentRepository.GetByProjectIdAsync(projectId, cancellationToken)).Select(MapToDto).ToArray();
    }

    public async Task<ProjectDocumentDto> CreateAsync(
        ProjectDocumentDto document,
        CancellationToken cancellationToken = default)
    {
        await EnsureProjectExistsAsync(document.ProjectId, cancellationToken);
        return MapToDto(await documentRepository.CreateAsync(new ProjectDocument
        {
            ProjectId = document.ProjectId,
            Title = document.Title.Trim()
        }, cancellationToken));
    }

    public async Task<ProjectDocumentDto?> UpdateAsync(
        uint id,
        ProjectDocumentDto document,
        CancellationToken cancellationToken = default)
    {
        if (await documentRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return null;
        }

        await EnsureProjectExistsAsync(document.ProjectId, cancellationToken);
        var updated = await documentRepository.UpdateAsync(id, new ProjectDocument
        {
            Id = id,
            ProjectId = document.ProjectId,
            Title = document.Title.Trim()
        }, cancellationToken);
        return updated is null ? null : MapToDto(updated);
    }

    public Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default) =>
        documentRepository.DeleteAsync(id, cancellationToken);

    private async Task EnsureProjectExistsAsync(uint projectId, CancellationToken cancellationToken)
    {
        if (await projectRepository.GetByIdAsync(projectId, cancellationToken) is null)
        {
            throw new AssociatedProjectNotFoundException(projectId);
        }
    }

    private static ProjectDocumentDto MapToDto(ProjectDocument document) => new()
    {
        Id = document.Id,
        ProjectId = document.ProjectId,
        Title = document.Title,
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };
}
