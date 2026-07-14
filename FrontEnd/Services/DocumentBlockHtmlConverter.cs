using System.Net;
using System.Text.Json;
using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class DocumentBlockHtmlConverter
{
    public string ToEditableHtml(IEnumerable<ProjectDocumentBlockApiDto> blocks) => string.Join("\n", blocks
        .OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).Select(Convert));

    private static string Convert(ProjectDocumentBlockApiDto block)
    {
        var content = block.Content ?? string.Empty;
        return block.BlockType switch
        {
            "rich_text" => content,
            "heading" => $"<h{GetInt(block.Settings, "level", 2)}>{Encode(content)}</h{GetInt(block.Settings, "level", 2)}>",
            "image" => $"<img src=\"{Attr(content)}\" alt=\"{Attr(GetString(block.Settings, "alt"))}\" data-caption=\"{Attr(GetString(block.Settings, "caption"))}\">",
            "gallery" => $"<figure class=\"document-gallery\">{GalleryHtml(block)}</figure>",
            "video" => $"<video controls src=\"{Attr(content)}\" title=\"{Attr(GetString(block.Settings, "title"))}\"></video>",
            "code" => $"<pre data-language=\"{Attr(GetString(block.Settings, "language"))}\"><code>{Encode(content)}</code></pre>",
            "quote" => $"<blockquote>{Encode(content)}</blockquote>",
            "button" => $"<a class=\"document-button\" href=\"{Attr(GetString(block.Settings, "url"))}\" target=\"{Attr(GetString(block.Settings, "target"))}\">{Encode(content)}</a>",
            "separator" => "<hr>",
            _ => content
        };
    }

    private static string GalleryHtml(ProjectDocumentBlockApiDto block)
    {
        try
        {
            var json = GetElement(block.Settings, "images");
            if (json is null || json.Value.ValueKind != JsonValueKind.Array) return block.Content ?? string.Empty;
            return string.Join(string.Empty, json.Value.EnumerateArray().Select(item => item.ValueKind == JsonValueKind.String
                ? $"<img src=\"{Attr(item.GetString())}\">"
                : $"<img src=\"{Attr(item.TryGetProperty("url", out var url) ? url.GetString() : null)}\" alt=\"{Attr(item.TryGetProperty("alt", out var alt) ? alt.GetString() : null)}\">"));
        }
        catch { return block.Content ?? string.Empty; }
    }

    private static JsonElement? GetElement(string? settings, string name) { if (string.IsNullOrWhiteSpace(settings)) return null; using var document = JsonDocument.Parse(settings); return document.RootElement.TryGetProperty(name, out var value) ? value.Clone() : null; }
    private static string? GetString(string? settings, string name) { try { return GetElement(settings, name)?.ToString(); } catch { return null; } }
    private static int GetInt(string? settings, string name, int fallback) => int.TryParse(GetString(settings, name), out var value) ? Math.Clamp(value, 1, 6) : fallback;
    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
    private static string Attr(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
