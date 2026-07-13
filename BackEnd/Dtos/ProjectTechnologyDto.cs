using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class ProjectTechnologyDto
{
    public uint ProjectId { get; init; }
    public string? ProjectTitle { get; init; }
    public string? ProjectSlug { get; init; }

    public uint TechnologyId { get; init; }
    public string? TechnologyName { get; init; }
    public string? TechnologySlug { get; init; }
    public uint CategoryId { get; init; }
    public string? CategoryName { get; init; }

    public bool IsPrimary { get; init; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }
}
