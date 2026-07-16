using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class ContactMessageDto
{
    public Guid Id { get; init; }
    [Required, StringLength(80, MinimumLength = 2)] public string SenderName { get; init; } = string.Empty;
    [Required, EmailAddress, StringLength(254)] public string SenderEmail { get; init; } = string.Empty;
    [Required, StringLength(120, MinimumLength = 3)] public string Subject { get; init; } = string.Empty;
    [Required, StringLength(2000, MinimumLength = 10)] public string Content { get; init; } = string.Empty;
    public DateTime ReceivedAt { get; init; }
    public bool IsRead { get; init; }
}
