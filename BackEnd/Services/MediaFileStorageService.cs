using BackEnd.Options;
using Microsoft.Extensions.Options;

namespace BackEnd.Services;

public sealed record StoredMediaFile(string MediaType, string OriginalFilename, string StoredFilename, string FilePath, string PublicUrl, string MimeType, ulong FileSize);

public sealed class MediaFileStorageService(IWebHostEnvironment environment, IOptions<MediaStorageOptions> options)
{
    private static readonly Dictionary<string, (string Type, string[] Extensions)> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ("image", [".jpg", ".jpeg"]), ["image/png"] = ("image", [".png"]),
        ["image/webp"] = ("image", [".webp"]), ["image/gif"] = ("image", [".gif"]),
        ["video/mp4"] = ("video", [".mp4"]), ["video/webm"] = ("video", [".webm"]),
        ["application/pdf"] = ("document", [".pdf"])
    };

    public string RootPath => Path.GetFullPath(Path.IsPathRooted(options.Value.RootPath)
        ? options.Value.RootPath : Path.Combine(environment.ContentRootPath, options.Value.RootPath));
    public string PublicPath => "/" + options.Value.PublicPath.Trim('/');
    public long MaxFileSizeBytes => options.Value.MaxFileSizeBytes;
    public long MaxRequestSizeBytes => options.Value.MaxRequestSizeBytes;
    public int MaxFilesPerUpload => options.Value.MaxFilesPerUpload;

    public async Task<StoredMediaFile> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0) throw new InvalidDataException("Le fichier est vide.");
        if (file.Length > MaxFileSizeBytes) throw new InvalidDataException($"Le fichier dépasse la limite de {MaxFileSizeBytes} octets.");
        if (!Allowed.TryGetValue(file.ContentType, out var allowed)) throw new InvalidDataException("Le type MIME du fichier n'est pas autorisé.");

        var submittedFilename = Path.GetFileName(file.FileName.Replace('\\', '/'));
        var extension = Path.GetExtension(submittedFilename).ToLowerInvariant();
        if (!allowed.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) throw new InvalidDataException("L'extension ne correspond pas au type MIME déclaré.");
        await using (var input = file.OpenReadStream())
        {
            var header = new byte[Math.Min(16, (int)file.Length)];
            _ = await input.ReadAsync(header, cancellationToken);
            if (!HasValidSignature(file.ContentType, header)) throw new InvalidDataException("La signature du fichier ne correspond pas à son type.");
        }

        Directory.CreateDirectory(RootPath);
        var storedFilename = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(RootPath, storedFilename);
        try
        {
            await using var input = file.OpenReadStream();
            await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            await input.CopyToAsync(output, cancellationToken);
        }
        catch
        {
            if (File.Exists(destination)) File.Delete(destination);
            throw;
        }

        return new(allowed.Type, NormalizeOriginalFilename(submittedFilename, extension), storedFilename,
            Path.GetRelativePath(environment.ContentRootPath, destination).Replace('\\', '/'),
            $"{PublicPath}/{storedFilename}", file.ContentType, checked((ulong)file.Length));
    }

    public Task DeleteAsync(string filePath)
    {
        var candidate = Path.GetFullPath(Path.IsPathRooted(filePath) ? filePath : Path.Combine(environment.ContentRootPath, filePath));
        if (!candidate.StartsWith(RootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return Task.CompletedTask;
        if (File.Exists(candidate)) File.Delete(candidate);
        return Task.CompletedTask;
    }

    private static bool HasValidSignature(string mime, byte[] h) => mime switch
    {
        "image/jpeg" => Starts(h, 0xFF, 0xD8, 0xFF),
        "image/png" => Starts(h, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
        "image/gif" => h.Length >= 6 && (System.Text.Encoding.ASCII.GetString(h, 0, 6) is "GIF87a" or "GIF89a"),
        "image/webp" => h.Length >= 12 && System.Text.Encoding.ASCII.GetString(h, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(h, 8, 4) == "WEBP",
        "video/mp4" => h.Length >= 8 && System.Text.Encoding.ASCII.GetString(h, 4, 4) == "ftyp",
        "video/webm" => Starts(h, 0x1A, 0x45, 0xDF, 0xA3),
        "application/pdf" => h.Length >= 5 && System.Text.Encoding.ASCII.GetString(h, 0, 5) == "%PDF-",
        _ => false
    };

    private static bool Starts(byte[] source, params byte[] expected) => source.Length >= expected.Length && expected.SequenceEqual(source.Take(expected.Length));

    private static string NormalizeOriginalFilename(string filename, string extension)
    {
        var normalized = new string(filename.Where(character => !char.IsControl(character)).ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(normalized)) normalized = $"media{extension}";
        return normalized.Length <= 255 ? normalized : normalized[..255];
    }
}
