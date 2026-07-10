namespace FrontEnd.Models;

public sealed class AdminProject
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string ImageUrl { get; set; }
    public required string Type { get; set; }
    public List<string> Languages { get; set; } = [];
    public List<string> Frameworks { get; set; } = [];

    public IEnumerable<string> Technologies => Languages.Concat(Frameworks);
}
