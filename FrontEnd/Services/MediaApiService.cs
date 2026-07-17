using System.Net.Http.Json;
using FrontEnd.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace FrontEnd.Services;

public sealed class MediaApiService(HttpClient httpClient) : IMediaApiService
{
    private const long MaxUploadSize = 26_214_400;
    public async Task<IReadOnlyList<MediaApiDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await httpClient.GetFromJsonAsync<MediaApiDto[]>("api/media", cancellationToken) ?? [])
        .Select(NormalizePublicUrl)
        .ToArray();

    public async Task<IReadOnlyList<MediaApiDto>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default) =>
        (await httpClient.GetFromJsonAsync<MediaApiDto[]>($"api/projects/{projectId}/media", cancellationToken) ?? [])
        .Select(NormalizePublicUrl)
        .ToArray();

    public async Task<MediaApiDto> UploadAsync(IBrowserFile file, uint? projectId, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        await using var source = file.OpenReadStream(MaxUploadSize, cancellationToken);
        await using var tracked = new ProgressReadStream(source, file.Size, progress);
        using var form = new MultipartFormDataContent();
        using var content = new StreamContent(tracked);
        content.Headers.ContentType = new(file.ContentType);
        form.Add(content, "files", file.Name);
        if (projectId.HasValue) form.Add(new StringContent(projectId.Value.ToString()), "projectId");
        using var response = await httpClient.PostAsync("api/media/upload", form, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var media = (await response.Content.ReadFromJsonAsync<MediaApiDto[]>(cancellationToken: cancellationToken))?.Single()
            ?? throw new InvalidOperationException("La réponse d'upload est vide.");
        return NormalizePublicUrl(media);
    }

    public async Task<MediaApiDto> UpdateAsync(uint id, MediaApiDto media, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"api/media/{id}", media, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var updatedMedia = await response.Content.ReadFromJsonAsync<MediaApiDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("La réponse de mise à jour est vide.");
        return NormalizePublicUrl(updatedMedia);
    }

    public async Task DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"api/media/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
        var detail = problem.TryGetProperty("detail", out var value) ? value.GetString() : null;
        throw new InvalidOperationException(detail ?? $"L'API a retourné {(int)response.StatusCode}.");
    }

    private MediaApiDto NormalizePublicUrl(MediaApiDto media)
    {
        if (string.IsNullOrWhiteSpace(media.PublicUrl)) return media;

        var publicUrl = media.PublicUrl.Trim();
        if (Uri.TryCreate(publicUrl, UriKind.Absolute, out var absoluteUrl))
        {
            media.PublicUrl = absoluteUrl.Scheme is "http" or "https" ? absoluteUrl.ToString() : null;
            return media;
        }

        if (httpClient.BaseAddress is { Scheme: "http" or "https" } baseAddress &&
            Uri.TryCreate(publicUrl, UriKind.Relative, out var relativeUrl))
        {
            media.PublicUrl = new Uri(baseAddress, relativeUrl).ToString();
            return media;
        }

        media.PublicUrl = null;
        return media;
    }

    private sealed class ProgressReadStream(Stream inner, long length, IProgress<int>? progress) : Stream
    {
        private long read;
        public override bool CanRead => inner.CanRead; public override bool CanSeek => false; public override bool CanWrite => false;
        public override long Length => length; public override long Position { get => read; set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush(); public override int Read(byte[] buffer, int offset, int count) { var n = inner.Read(buffer, offset, count); Report(n); return n; }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) { var n = await inner.ReadAsync(buffer, cancellationToken); Report(n); return n; }
        private void Report(int count) { read += count; progress?.Report(length == 0 ? 0 : (int)(read * 100 / length)); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException(); public override void SetLength(long value) => throw new NotSupportedException(); public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
