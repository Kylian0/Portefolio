namespace FrontEnd.Models;

public sealed class MediaApiDto
{
    public uint Id { get; set; }
    public uint? ProjectId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string OriginalFilename { get; set; } = string.Empty;
    public string StoredFilename { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? PublicUrl { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public ulong FileSize { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
}
