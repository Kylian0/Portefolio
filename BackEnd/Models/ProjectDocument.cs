namespace BackEnd.Models;

public sealed class ProjectDocument
{
    public uint Id { get; init; }
    public uint ProjectId { get; init; }
    public required string Title { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
