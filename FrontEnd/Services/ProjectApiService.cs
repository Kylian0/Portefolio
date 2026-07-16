using System.Net.Http.Json;
using System.Text.Json;
using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class ProjectApiService(HttpClient httpClient) : IProjectApiService
{
    public async Task<IReadOnlyList<ProjectApiDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await httpClient.GetFromJsonAsync<ProjectApiDto[]>("api/projects", cancellationToken) ?? [];
        foreach (var project in projects)
        {
            if (!string.IsNullOrWhiteSpace(project.ThumbnailUrl) &&
                Uri.TryCreate(project.ThumbnailUrl, UriKind.Relative, out var relativeUrl))
            {
                project.ThumbnailUrl = new Uri(httpClient.BaseAddress!, relativeUrl).ToString();
            }
        }

        return projects;
    }

    public async Task<ProjectApiDto> UpdateAsync(uint id, ProjectApiDto project, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"api/projects/{id}", project, cancellationToken);
        if (!response.IsSuccessStatusCode) throw await CreateExceptionAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ProjectApiDto>(cancellationToken: cancellationToken)
            ?? throw new ProjectApiException("La réponse de l'API est vide.");
    }

    public async Task<ProjectApiDto> CreateAsync(ProjectApiDto project, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/projects", project, cancellationToken);
        if (!response.IsSuccessStatusCode) throw await CreateExceptionAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ProjectApiDto>(cancellationToken: cancellationToken) ?? throw new ProjectApiException("La réponse de l'API est vide.");
    }

    public async Task DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"api/projects/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode) throw await CreateExceptionAsync(response, cancellationToken);
    }

    private static async Task<ProjectApiException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = json.RootElement;
            var errors = new List<string>();
            if (root.TryGetProperty("errors", out var validationErrors))
            {
                foreach (var property in validationErrors.EnumerateObject())
                    errors.AddRange(property.Value.EnumerateArray().Select(item => item.GetString()).Where(item => !string.IsNullOrWhiteSpace(item))!);
            }
            if (errors.Count == 0 && root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString())) errors.Add(detail.GetString()!);
            if (errors.Count == 0 && root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString())) errors.Add(title.GetString()!);
            return new(errors.FirstOrDefault() ?? $"Erreur API ({(int)response.StatusCode}).", errors);
        }
        catch (JsonException)
        {
            return new($"Erreur API ({(int)response.StatusCode}).");
        }
    }
}
