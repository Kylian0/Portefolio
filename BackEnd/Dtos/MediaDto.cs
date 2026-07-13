using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class MediaDto
{
    public uint Id { get; init; }
    public uint? ProjectId { get; init; }

    [Required]
    [RegularExpression("^(image|video|document)$")]
    public string MediaType { get; init; } = string.Empty;

    [Required, StringLength(255)]
    public string OriginalFilename { get; init; } = string.Empty;

    [Required, StringLength(255)]
    public string StoredFilename { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string FilePath { get; init; } = string.Empty;

    [StringLength(500), Url]
    public string? PublicUrl { get; init; }

    [Required, StringLength(150)]
    public string MimeType { get; init; } = string.Empty;

    [Range(typeof(ulong), "1", "18446744073709551615")]
    public ulong FileSize { get; init; }

    [StringLength(500)]
    public string? AltText { get; init; }

    [StringLength(500)]
    public string? Caption { get; init; }

    public DateTime CreatedAt { get; init; }
}
