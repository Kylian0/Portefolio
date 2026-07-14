using System.Net.Http.Json;
using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class ProjectTechnologyApiService(HttpClient httpClient) : IProjectTechnologyApiService
{
    private readonly Dictionary<uint, Task<IReadOnlyList<ProjectTechnologyApiDto>>> cache = [];
    private Task<IReadOnlyList<TechnologyCategoryApiDto>>? categoriesRequest;
    private Task<IReadOnlyList<TechnologyApiDto>>? technologiesRequest;

    public async Task<IReadOnlyList<TechnologyCategoryApiDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        categoriesRequest ??= LoadCategoriesAsync(cancellationToken);
        try { return await categoriesRequest; }
        catch { categoriesRequest = null; throw; }
    }

    public async Task<IReadOnlyList<TechnologyApiDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        technologiesRequest ??= LoadTechnologiesAsync(cancellationToken);
        try { return await technologiesRequest; }
        catch { technologiesRequest = null; throw; }
    }

    public async Task<IReadOnlyList<ProjectTechnologyApiDto>> GetByProjectIdAsync(
        uint projectId,
        CancellationToken cancellationToken = default)
    {
        if (!cache.TryGetValue(projectId, out var request))
        {
            request = LoadAsync(projectId, cancellationToken);
            cache[projectId] = request;
        }

        try
        {
            return await request;
        }
        catch
        {
            cache.Remove(projectId);
            throw;
        }
    }

    private async Task<IReadOnlyList<ProjectTechnologyApiDto>> LoadAsync(
        uint projectId,
        CancellationToken cancellationToken) =>
        await httpClient.GetFromJsonAsync<ProjectTechnologyApiDto[]>(
            $"api/projects/{projectId}/technologies",
            cancellationToken) ?? [];

    private async Task<IReadOnlyList<TechnologyCategoryApiDto>> LoadCategoriesAsync(CancellationToken cancellationToken) =>
        await httpClient.GetFromJsonAsync<TechnologyCategoryApiDto[]>("api/technology-categories", cancellationToken) ?? [];

    private async Task<IReadOnlyList<TechnologyApiDto>> LoadTechnologiesAsync(CancellationToken cancellationToken) =>
        await httpClient.GetFromJsonAsync<TechnologyApiDto[]>("api/technologies", cancellationToken) ?? [];
}
