using System.Net.Http.Json;
using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class ProjectDocumentationService(
    HttpClient httpClient,
    IProjectApiService projectApiService,
    IMediaApiService mediaApiService,
    IProjectTechnologyApiService technologyApiService,
    IProjectLearningApiService learningApiService) : IProjectDocumentationService
{
    public Task<ProjectDocumentApiDto> CreateDocumentAsync(ProjectDocumentApiDto document, CancellationToken cancellationToken = default) =>
        SendAsync<ProjectDocumentApiDto>(HttpMethod.Post, "api/project-documents", document, cancellationToken);

    public Task<ProjectDocumentApiDto> UpdateDocumentAsync(ProjectDocumentApiDto document, CancellationToken cancellationToken = default) =>
        SendAsync<ProjectDocumentApiDto>(HttpMethod.Put, $"api/project-documents/{document.Id}", document, cancellationToken);

    public Task DeleteDocumentAsync(uint id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/project-documents/{id}", cancellationToken);

    public Task<ProjectDocumentBlockApiDto> CreateBlockAsync(ProjectDocumentBlockApiDto block, CancellationToken cancellationToken = default) =>
        SendAsync<ProjectDocumentBlockApiDto>(HttpMethod.Post, "api/project-document-blocks", block, cancellationToken);

    public Task<ProjectDocumentBlockApiDto> UpdateBlockAsync(ProjectDocumentBlockApiDto block, CancellationToken cancellationToken = default) =>
        SendAsync<ProjectDocumentBlockApiDto>(HttpMethod.Put, $"api/project-document-blocks/{block.Id}", block, cancellationToken);

    public Task DeleteBlockAsync(uint id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/project-document-blocks/{id}", cancellationToken);

    public Task<IReadOnlyList<ProjectDocumentBlockApiDto>> SynchronizeAsync(uint documentId, string title, string html, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ProjectDocumentBlockApiDto>>(HttpMethod.Put, $"api/project-documents/{documentId}/content", new { title, html }, cancellationToken);

    public async Task<ProjectDocumentationData> GetByProjectSlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var projects = await projectApiService.GetAllAsync(cancellationToken);
        var project = projects.FirstOrDefault(item =>
            string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

        if (project is null)
        {
            return new(null, null, [], [], [], []);
        }

        var media = await mediaApiService.GetByProjectIdAsync(project.Id, cancellationToken);
        IReadOnlyList<ProjectTechnologyApiDto> technologies = [];
        var technologyLoadFailed = false;
        IReadOnlyList<ProjectLearningApiDto> learnings = [];
        var learningLoadFailed = false;
        try
        {
            technologies = await technologyApiService.GetByProjectIdAsync(project.Id, cancellationToken);
        }
        catch
        {
            technologyLoadFailed = true;
        }

        try
        {
            learnings = (await learningApiService.GetByProjectIdAsync(project.Id, cancellationToken))
                .Where(item => !string.IsNullOrWhiteSpace(item.Content))
                .ToArray();
        }
        catch
        {
            learningLoadFailed = true;
        }

        var documents = await httpClient.GetFromJsonAsync<ProjectDocumentApiDto[]>(
            $"api/projects/{project.Id}/documents",
            cancellationToken) ?? [];
        var document = documents.FirstOrDefault();

        if (document is null)
        {
            return new(project, null, [], media, technologies, learnings, technologyLoadFailed, learningLoadFailed);
        }

        var blocks = await httpClient.GetFromJsonAsync<ProjectDocumentBlockApiDto[]>(
            $"api/project-documents/{document.Id}/blocks",
            cancellationToken) ?? [];

        return new(project, document, blocks, media, technologies, learnings, technologyLoadFailed, learningLoadFailed);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body) };
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) throw await ReadExceptionAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
            ?? throw new ProjectApiException("La réponse de l'API est vide.");
    }

    private async Task DeleteAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await httpClient.DeleteAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode) throw await ReadExceptionAsync(response, cancellationToken);
    }

    private static async Task<ProjectApiException> ReadExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
            var errors = new List<string>();
            if (problem.TryGetProperty("errors", out var validation))
                foreach (var property in validation.EnumerateObject()) errors.AddRange(property.Value.EnumerateArray().Select(item => item.GetString()).Where(item => !string.IsNullOrWhiteSpace(item))!);
            if (errors.Count == 0 && problem.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString())) errors.Add(detail.GetString()!);
            return new(errors.FirstOrDefault() ?? $"Erreur API ({(int)response.StatusCode}).", errors);
        }
        catch { return new($"Erreur API ({(int)response.StatusCode})."); }
    }
}
