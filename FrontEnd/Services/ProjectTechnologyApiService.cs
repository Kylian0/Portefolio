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

    public async Task<TechnologyCategoryApiDto> SaveCategoryAsync(TechnologyCategoryApiDto item,CancellationToken token=default){using var response=item.Id==0?await httpClient.PostAsJsonAsync("api/technology-categories",item,token):await httpClient.PutAsJsonAsync($"api/technology-categories/{item.Id}",item,token);response.EnsureSuccessStatusCode();categoriesRequest=null;return (await response.Content.ReadFromJsonAsync<TechnologyCategoryApiDto>(cancellationToken:token))!;}
    public async Task DeleteCategoryAsync(uint id,CancellationToken token=default){using var response=await httpClient.DeleteAsync($"api/technology-categories/{id}",token);response.EnsureSuccessStatusCode();categoriesRequest=null;}
    public async Task<TechnologyApiDto> SaveTechnologyAsync(TechnologyApiDto item,CancellationToken token=default){using var response=item.Id==0?await httpClient.PostAsJsonAsync("api/technologies",item,token):await httpClient.PutAsJsonAsync($"api/technologies/{item.Id}",item,token);response.EnsureSuccessStatusCode();technologiesRequest=null;return (await response.Content.ReadFromJsonAsync<TechnologyApiDto>(cancellationToken:token))!;}
    public async Task DeleteTechnologyAsync(uint id,CancellationToken token=default){using var response=await httpClient.DeleteAsync($"api/technologies/{id}",token);response.EnsureSuccessStatusCode();technologiesRequest=null;}
    public async Task<ProjectTechnologyApiDto> AddToProjectAsync(ProjectTechnologyApiDto item,CancellationToken token=default){using var response=await httpClient.PostAsJsonAsync("api/project-technologies",item,token);response.EnsureSuccessStatusCode();cache.Remove(item.ProjectId);return (await response.Content.ReadFromJsonAsync<ProjectTechnologyApiDto>(cancellationToken:token))!;}
    public async Task<ProjectTechnologyApiDto> UpdateProjectTechnologyAsync(ProjectTechnologyApiDto item,CancellationToken token=default){using var response=await httpClient.PutAsJsonAsync($"api/projects/{item.ProjectId}/technologies/{item.TechnologyId}",item,token);response.EnsureSuccessStatusCode();cache.Remove(item.ProjectId);return (await response.Content.ReadFromJsonAsync<ProjectTechnologyApiDto>(cancellationToken:token))!;}
    public async Task RemoveFromProjectAsync(uint projectId,uint technologyId,CancellationToken token=default){using var response=await httpClient.DeleteAsync($"api/projects/{projectId}/technologies/{technologyId}",token);response.EnsureSuccessStatusCode();cache.Remove(projectId);}
}
