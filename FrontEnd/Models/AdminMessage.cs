namespace FrontEnd.Models;

public sealed class AdminMessage
{
    public required Guid Id { get; init; }
    public required string SenderName { get; init; }
    public required string SenderEmail { get; init; }
    public required string Subject { get; init; }
    public required string Content { get; init; }
    public required DateTime ReceivedAt { get; init; }
    public bool IsRead { get; set; }
}
