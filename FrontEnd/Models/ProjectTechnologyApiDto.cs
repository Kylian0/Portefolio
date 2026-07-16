namespace FrontEnd.Models;

public sealed class ProjectTechnologyApiDto
{
    public uint ProjectId { get; set; }
    public string? ProjectTitle { get; init; }
    public string? ProjectSlug { get; init; }
    public uint TechnologyId { get; set; }
    public string? TechnologyName { get; init; }
    public string? TechnologySlug { get; init; }
    public uint CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public bool IsPrimary { get; set; } public int DisplayOrder { get; set; }
}
