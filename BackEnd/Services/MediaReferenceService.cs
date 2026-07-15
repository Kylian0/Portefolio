using System.Text.Json;
using BackEnd.Models;
using System.Text.RegularExpressions;

namespace BackEnd.Services;

public sealed class MediaReferenceService(ProjectDocumentBlock blockModel)
{
    public async Task<bool> IsReferencedAsync(uint mediaId, CancellationToken cancellationToken)
    {
        foreach (var block in await blockModel.GetAllAsync(cancellationToken))
        {
            if (uint.TryParse(block.Content, out var contentId) && contentId == mediaId) return true;
            if (!string.IsNullOrWhiteSpace(block.Content) && Regex.IsMatch(block.Content, $"data-media-id\\s*=\\s*[\"']{mediaId}[\"']", RegexOptions.IgnoreCase)) return true;
            if (string.IsNullOrWhiteSpace(block.Settings)) continue;
            try { using var json = JsonDocument.Parse(block.Settings); if (ContainsReference(json.RootElement, mediaId, null)) return true; }
            catch (JsonException) { }
        }
        return false;
    }

    private static bool ContainsReference(JsonElement value, uint id, string? property)
    {
        if (value.ValueKind == JsonValueKind.Object)
            return value.EnumerateObject().Any(item => ContainsReference(item.Value, id, item.Name));
        if (value.ValueKind == JsonValueKind.Array)
            return value.EnumerateArray().Any(item => ContainsReference(item, id, property));
        var mediaProperty = property is not null && (property.Equals("mediaId", StringComparison.OrdinalIgnoreCase) || property.Equals("mediaIds", StringComparison.OrdinalIgnoreCase));
        return mediaProperty && ((value.ValueKind == JsonValueKind.Number && value.TryGetUInt32(out var number) && number == id) ||
            (value.ValueKind == JsonValueKind.String && uint.TryParse(value.GetString(), out number) && number == id));
    }
}
