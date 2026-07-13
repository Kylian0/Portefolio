using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class ProjectCreateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "The slug must contain lowercase letters, numbers, and single hyphens only.")]
    public string Slug { get; init; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ShortDescription { get; init; } = string.Empty;

    [StringLength(500)]
    [Url]
    public string? ThumbnailUrl { get; init; }

    [StringLength(500)]
    [Url]
    public string? RepositoryUrl { get; init; }

    [StringLength(500)]
    [Url]
    public string? DemoUrl { get; init; }

    [RegularExpression("^(draft|published|archived)$")]
    public string Status { get; init; } = "draft";

    public bool IsFeatured { get; init; }
    public int DisplayOrder { get; init; }
    public DateOnly? StartedAt { get; init; }
    public DateOnly? CompletedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}
