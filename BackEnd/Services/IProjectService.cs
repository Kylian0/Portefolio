using BackEnd.Dtos;

namespace BackEnd.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectReadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectReadDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<ProjectReadDto> CreateAsync(ProjectCreateDto project, CancellationToken cancellationToken = default);
    Task<ProjectReadDto?> UpdateAsync(uint id, ProjectUpdateDto project, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
