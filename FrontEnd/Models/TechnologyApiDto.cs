namespace FrontEnd.Models;

public sealed class TechnologyApiDto
{
    public uint Id { get; set; } public uint CategoryId { get; set; } public string? CategoryName { get; set; } public string? CategorySlug { get; set; } public string? Name { get; set; } public string? Slug { get; set; }
    public string? IconUrl { get; set; } public string? OfficialUrl { get; set; }
}
