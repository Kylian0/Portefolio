namespace FrontEnd.Models;

public sealed class ProjectLearningApiDto
{
    public uint Id { get; set; } public uint ProjectId { get; set; }
    public string? ProjectTitle { get; init; }
    public string? Content { get; set; } public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; init; }
}
