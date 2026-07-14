namespace FrontEnd.Models;

public sealed class ProjectDocumentApiDto
{
    public uint Id { get; set; }
    public uint ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
