using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IProjectLearningApiService
{
    Task<IReadOnlyList<ProjectLearningApiDto>> GetByProjectIdAsync(
        uint projectId,
        CancellationToken cancellationToken = default);
}
