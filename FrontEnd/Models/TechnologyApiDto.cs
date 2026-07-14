namespace FrontEnd.Models;

public sealed class TechnologyApiDto
{
    public uint Id { get; init; }
    public uint CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategorySlug { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
}
