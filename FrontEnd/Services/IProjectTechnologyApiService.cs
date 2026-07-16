using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IProjectTechnologyApiService
{
    Task<IReadOnlyList<TechnologyCategoryApiDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechnologyApiDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectTechnologyApiDto>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default);
    Task<TechnologyCategoryApiDto> SaveCategoryAsync(TechnologyCategoryApiDto item, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(uint id, CancellationToken cancellationToken = default);
    Task<TechnologyApiDto> SaveTechnologyAsync(TechnologyApiDto item, CancellationToken cancellationToken = default);
    Task DeleteTechnologyAsync(uint id, CancellationToken cancellationToken = default);
    Task<ProjectTechnologyApiDto> AddToProjectAsync(ProjectTechnologyApiDto item, CancellationToken cancellationToken = default);
    Task<ProjectTechnologyApiDto> UpdateProjectTechnologyAsync(ProjectTechnologyApiDto item, CancellationToken cancellationToken = default);
    Task RemoveFromProjectAsync(uint projectId, uint technologyId, CancellationToken cancellationToken = default);
}
