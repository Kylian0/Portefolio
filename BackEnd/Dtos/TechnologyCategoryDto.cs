using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class TechnologyCategoryDto
{
    public uint Id { get; init; }

    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(100)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "The slug must contain lowercase letters, numbers, and single hyphens only.")]
    public string Slug { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }

    public DateTime CreatedAt { get; init; }
}
