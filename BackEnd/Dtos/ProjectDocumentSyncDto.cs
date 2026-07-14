using System.ComponentModel.DataAnnotations;

namespace BackEnd.Dtos;

public sealed class ProjectDocumentSyncDto
{
    [Required, StringLength(200)] public string Title { get; init; } = "Documentation";
    [Required] public string Html { get; init; } = string.Empty;
}
