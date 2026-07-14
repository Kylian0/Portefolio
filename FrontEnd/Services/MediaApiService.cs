using System.Net.Http.Json;
using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class MediaApiService(HttpClient httpClient) : IMediaApiService
{
    public async Task<IReadOnlyList<MediaApiDto>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<MediaApiDto[]>($"api/projects/{projectId}/media", cancellationToken) ?? [];
}
