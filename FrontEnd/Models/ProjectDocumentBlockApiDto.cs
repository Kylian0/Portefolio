namespace FrontEnd.Models;

public sealed class ProjectDocumentBlockApiDto
{
    public uint Id { get; set; }
    public uint DocumentId { get; set; }
    public string BlockType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Settings { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
