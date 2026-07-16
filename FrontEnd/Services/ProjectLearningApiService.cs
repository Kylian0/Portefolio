using System.Net.Http.Json;
using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class ProjectLearningApiService(HttpClient httpClient) : IProjectLearningApiService
{
    public async Task<IReadOnlyList<ProjectLearningApiDto>> GetByProjectIdAsync(
        uint projectId,
        CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<ProjectLearningApiDto[]>(
            $"api/projects/{projectId}/learnings",
            cancellationToken) ?? [];
    public async Task<ProjectLearningApiDto> SaveAsync(ProjectLearningApiDto item,CancellationToken token=default){using var response=item.Id==0?await httpClient.PostAsJsonAsync("api/project-learnings",item,token):await httpClient.PutAsJsonAsync($"api/project-learnings/{item.Id}",item,token);response.EnsureSuccessStatusCode();return (await response.Content.ReadFromJsonAsync<ProjectLearningApiDto>(cancellationToken:token))!;}
    public async Task DeleteAsync(uint id,CancellationToken token=default){using var response=await httpClient.DeleteAsync($"api/project-learnings/{id}",token);response.EnsureSuccessStatusCode();}
}
