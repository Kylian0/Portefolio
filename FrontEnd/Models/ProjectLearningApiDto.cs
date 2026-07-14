namespace FrontEnd.Models;

public sealed class ProjectLearningApiDto
{
    public uint Id { get; init; }
    public uint ProjectId { get; init; }
    public string? ProjectTitle { get; init; }
    public string? Content { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime CreatedAt { get; init; }
}
