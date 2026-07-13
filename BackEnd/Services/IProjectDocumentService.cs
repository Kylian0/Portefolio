using BackEnd.Dtos;

namespace BackEnd.Services;

public interface IProjectDocumentService
{
    Task<IReadOnlyList<ProjectDocumentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectDocumentDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectDocumentDto>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default);
    Task<ProjectDocumentDto> CreateAsync(ProjectDocumentDto document, CancellationToken cancellationToken = default);
    Task<ProjectDocumentDto?> UpdateAsync(uint id, ProjectDocumentDto document, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
