using BackEnd.Dtos;
using BackEnd.Models;
using BackEnd.Repositories;

namespace BackEnd.Services;

public sealed class ProjectService(IProjectRepository projectRepository) : IProjectService
{
    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await projectRepository.GetAllAsync(cancellationToken);
        return projects.Select(MapToDto).ToArray();
    }

    public async Task<ProjectDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        return project is null ? null : MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(ProjectDto project, CancellationToken cancellationToken = default)
    {
        var entity = new Project
        {
            Title = project.Title.Trim(),
            Slug = project.Slug.Trim(),
            ShortDescription = project.ShortDescription.Trim(),
            ThumbnailUrl = project.ThumbnailUrl,
            RepositoryUrl = project.RepositoryUrl,
            DemoUrl = project.DemoUrl,
            Status = project.Status,
            IsFeatured = project.IsFeatured,
            DisplayOrder = project.DisplayOrder,
            StartedAt = project.StartedAt,
            CompletedAt = project.CompletedAt,
            PublishedAt = project.PublishedAt
        };

        return MapToDto(await projectRepository.CreateAsync(entity, cancellationToken));
    }

    public async Task<ProjectDto?> UpdateAsync(
        uint id,
        ProjectDto project,
        CancellationToken cancellationToken = default)
    {
        var entity = new Project
        {
            Id = id,
            Title = project.Title.Trim(),
            Slug = project.Slug.Trim(),
            ShortDescription = project.ShortDescription.Trim(),
            ThumbnailUrl = project.ThumbnailUrl,
            RepositoryUrl = project.RepositoryUrl,
            DemoUrl = project.DemoUrl,
            Status = project.Status,
            IsFeatured = project.IsFeatured,
            DisplayOrder = project.DisplayOrder,
            StartedAt = project.StartedAt,
            CompletedAt = project.CompletedAt,
            PublishedAt = project.PublishedAt
        };

        var updatedProject = await projectRepository.UpdateAsync(id, entity, cancellationToken);
        return updatedProject is null ? null : MapToDto(updatedProject);
    }

    public Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default) =>
        projectRepository.DeleteAsync(id, cancellationToken);

    private static ProjectDto MapToDto(Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Slug = project.Slug,
        ShortDescription = project.ShortDescription,
        ThumbnailUrl = project.ThumbnailUrl,
        RepositoryUrl = project.RepositoryUrl,
        DemoUrl = project.DemoUrl,
        Status = project.Status,
        IsFeatured = project.IsFeatured,
        DisplayOrder = project.DisplayOrder,
        StartedAt = project.StartedAt,
        CompletedAt = project.CompletedAt,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
        PublishedAt = project.PublishedAt
    };
}
