using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class AdminLoginDto
{
    [Required, StringLength(256)] public string Username { get; init; } = string.Empty;
    [Required, StringLength(200)] public string Password { get; init; } = string.Empty;
}
