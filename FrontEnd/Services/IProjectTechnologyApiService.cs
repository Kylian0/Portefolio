using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IProjectTechnologyApiService
{
    Task<IReadOnlyList<TechnologyCategoryApiDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechnologyApiDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectTechnologyApiDto>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default);
}
