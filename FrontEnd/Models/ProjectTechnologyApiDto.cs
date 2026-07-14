namespace FrontEnd.Models;

public sealed class ProjectTechnologyApiDto
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
    public int DisplayOrder { get; init; }
}
