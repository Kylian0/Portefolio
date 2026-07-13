using BackEnd.Models;

namespace BackEnd.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project?> UpdateAsync(uint id, Project project, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
