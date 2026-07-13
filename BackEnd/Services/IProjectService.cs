using BackEnd.Dtos;

namespace BackEnd.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<ProjectDto> CreateAsync(ProjectDto project, CancellationToken cancellationToken = default);
    Task<ProjectDto?> UpdateAsync(uint id, ProjectDto project, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
