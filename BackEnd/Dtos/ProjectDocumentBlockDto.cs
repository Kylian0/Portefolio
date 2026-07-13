using System.ComponentModel.DataAnnotations;
using BackEnd.Validation;

namespace BackEnd.Dtos;

public sealed class ProjectDocumentBlockDto
{
    public uint Id { get; init; }

    [Range(1, uint.MaxValue)]
    public uint DocumentId { get; init; }

    [Required]
    [RegularExpression("^(rich_text|heading|image|gallery|video|code|quote|button|separator)$")]
    public string BlockType { get; init; } = string.Empty;

    public string? Content { get; init; }

    [ValidJson(ErrorMessage = "Settings must contain valid JSON.")]
    public string? Settings { get; init; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
