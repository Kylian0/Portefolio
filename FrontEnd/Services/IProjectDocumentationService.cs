using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IProjectDocumentationService
{
    Task<ProjectDocumentationData> GetByProjectSlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ProjectDocumentApiDto> CreateDocumentAsync(ProjectDocumentApiDto document, CancellationToken cancellationToken = default);
    Task<ProjectDocumentApiDto> UpdateDocumentAsync(ProjectDocumentApiDto document, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(uint id, CancellationToken cancellationToken = default);
    Task<ProjectDocumentBlockApiDto> CreateBlockAsync(ProjectDocumentBlockApiDto block, CancellationToken cancellationToken = default);
    Task<ProjectDocumentBlockApiDto> UpdateBlockAsync(ProjectDocumentBlockApiDto block, CancellationToken cancellationToken = default);
    Task DeleteBlockAsync(uint id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectDocumentBlockApiDto>> SynchronizeAsync(uint documentId, string title, string html, CancellationToken cancellationToken = default);
}
