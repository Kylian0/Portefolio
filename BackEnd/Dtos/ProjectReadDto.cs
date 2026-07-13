namespace BackEnd.Dtos;

public sealed record ProjectReadDto(
    uint Id,
    string Title,
    string Slug,
    string ShortDescription,
    string? ThumbnailUrl,
    string? RepositoryUrl,
    string? DemoUrl,
    string Status,
    bool IsFeatured,
    int DisplayOrder,
    DateOnly? StartedAt,
    DateOnly? CompletedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);
