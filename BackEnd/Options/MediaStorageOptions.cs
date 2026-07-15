namespace BackEnd.Options;

public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";
    public string RootPath { get; set; } = "uploads";
    public string PublicPath { get; set; } = "/uploads";
    public long MaxFileSizeBytes { get; set; } = 26_214_400;
}
