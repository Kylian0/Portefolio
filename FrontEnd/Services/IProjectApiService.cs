using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IProjectApiService
{
    Task<IReadOnlyList<ProjectApiDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectApiDto> CreateAsync(ProjectApiDto project, CancellationToken cancellationToken = default);
    Task<ProjectApiDto> UpdateAsync(uint id, ProjectApiDto project, CancellationToken cancellationToken = default);
    Task DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
