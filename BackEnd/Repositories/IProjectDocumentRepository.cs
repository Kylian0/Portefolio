using BackEnd.Models;

namespace BackEnd.Repositories;

public interface IProjectDocumentRepository
{
    Task<IReadOnlyList<ProjectDocument>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectDocument?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectDocument>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default);
    Task<ProjectDocument> CreateAsync(ProjectDocument document, CancellationToken cancellationToken = default);
    Task<ProjectDocument?> UpdateAsync(uint id, ProjectDocument document, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
