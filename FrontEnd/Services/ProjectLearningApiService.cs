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
}
