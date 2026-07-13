using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class TechnologyDto
{
    public uint Id { get; init; }

    [Range(1, uint.MaxValue)]
    public uint CategoryId { get; init; }

    public string? CategoryName { get; init; }
    public string? CategorySlug { get; init; }

    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(100)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "The slug must contain lowercase letters, numbers, and single hyphens only.")]
    public string Slug { get; init; } = string.Empty;

    [StringLength(500), Url]
    public string? IconUrl { get; init; }

    [StringLength(500), Url]
    public string? OfficialUrl { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
