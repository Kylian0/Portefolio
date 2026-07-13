using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class ProjectDocumentDto
{
    public uint Id { get; init; }

    [Range(1, uint.MaxValue)]
    public uint ProjectId { get; init; }

    [Required]
    [StringLength(200)]
    public string Title { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
