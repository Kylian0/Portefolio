namespace BackEnd.Models;

public sealed class Project
{
    public uint Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required string ShortDescription { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? RepositoryUrl { get; init; }
    public string? DemoUrl { get; init; }
    public required string Status { get; init; }
    public bool IsFeatured { get; init; }
    public int DisplayOrder { get; init; }
    public DateOnly? StartedAt { get; init; }
    public DateOnly? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}
