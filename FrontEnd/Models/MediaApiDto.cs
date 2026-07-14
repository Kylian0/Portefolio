namespace FrontEnd.Models;

public sealed class MediaApiDto
{
    public uint Id { get; init; }
    public uint? ProjectId { get; init; }
    public string MediaType { get; init; } = string.Empty;
    public string OriginalFilename { get; init; } = string.Empty;
    public string StoredFilename { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string? PublicUrl { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public ulong FileSize { get; init; }
    public string? AltText { get; init; }
    public string? Caption { get; init; }
    public DateTime CreatedAt { get; init; }
}
