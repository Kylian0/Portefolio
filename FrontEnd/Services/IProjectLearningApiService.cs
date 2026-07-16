using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IProjectLearningApiService
{
    Task<IReadOnlyList<ProjectLearningApiDto>> GetByProjectIdAsync(
        uint projectId,
        CancellationToken cancellationToken = default);
    Task<ProjectLearningApiDto> SaveAsync(ProjectLearningApiDto item,CancellationToken cancellationToken=default);
    Task DeleteAsync(uint id,CancellationToken cancellationToken=default);
}
