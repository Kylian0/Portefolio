namespace FrontEnd.Models;

public sealed class TechnologyCategoryApiDto
{
    public uint Id { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public int DisplayOrder { get; init; }
}
