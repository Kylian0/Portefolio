using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class ProjectLearningDto
{
    public uint Id { get; init; }

    [Range(1, uint.MaxValue)]
    public uint ProjectId { get; init; }

    public string? ProjectTitle { get; init; }

    [Required, StringLength(500)]
    public string Content { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }

    public DateTime CreatedAt { get; init; }
}
