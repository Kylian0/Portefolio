using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using BackEnd.Models;
using Ganss.Xss;

namespace BackEnd.Services;

public sealed class HtmlDocumentBlockConverter
{
    private readonly HtmlSanitizer sanitizer = CreateSanitizer();

    public IReadOnlyList<ProjectDocumentBlock> Convert(uint documentId, string html)
    {
        var clean = sanitizer.Sanitize(html ?? string.Empty);
        var document = new HtmlParser().ParseDocument($"<body>{clean}</body>");
        var blocks = new List<ProjectDocumentBlock>();
        if (document.Body is null) return [];
        foreach (var node in document.Body.ChildNodes)
        {
            var block = ConvertNode(documentId, node, blocks.Count);
            if (block is not null) blocks.Add(block);
        }
        return blocks;
    }

    public ProjectDocumentBlock SanitizeBlock(ProjectDocumentBlock block) => new()
    {
        Id = block.Id,
        DocumentId = block.DocumentId,
        BlockType = block.BlockType,
        Content = block.BlockType == "rich_text" && block.Content is not null ? sanitizer.Sanitize(block.Content) : block.Content,
        Settings = block.Settings,
        DisplayOrder = block.DisplayOrder,
        CreatedAt = block.CreatedAt,
        UpdatedAt = block.UpdatedAt
    };

    private static ProjectDocumentBlock? ConvertNode(uint documentId, INode node, int order)
    {
        if (node is IText text && string.IsNullOrWhiteSpace(text.Data)) return null;
        if (node is not IElement element) return Rich(documentId, node.TextContent, order);
        var tag = element.TagName.ToLowerInvariant();
        return tag switch
        {
            "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => Block(documentId, "heading", element.TextContent, JsonSerializer.Serialize(new { level = int.Parse(tag[1..]) }), order),
            "img" => Block(documentId, "image", element.GetAttribute("src"), JsonSerializer.Serialize(new { mediaId = GetMediaId(element), alt = element.GetAttribute("alt"), caption = element.GetAttribute("data-caption") }), order),
            "figure" when element.ClassList.Contains("document-gallery") => Block(documentId, "gallery", element.InnerHtml, JsonSerializer.Serialize(new { mediaIds = element.QuerySelectorAll("img").Select(GetMediaId).Where(id => id.HasValue).Select(id => id!.Value).ToArray(), images = element.QuerySelectorAll("img").Select(img => new { mediaId = GetMediaId(img), url = img.GetAttribute("src"), alt = img.GetAttribute("alt"), caption = img.GetAttribute("data-caption") }).ToArray() }), order),
            "video" => SafeVideoUrl(element.GetAttribute("src") ?? element.QuerySelector("source")?.GetAttribute("src")) is { } videoUrl
                ? Block(documentId, "video", videoUrl, JsonSerializer.Serialize(new { mediaId = GetMediaId(element), title = element.GetAttribute("title") }), order)
                : Rich(documentId, "<p>Vidéo externe non autorisée.</p>", order),
            "pre" => Block(documentId, "code", element.TextContent, JsonSerializer.Serialize(new { language = element.GetAttribute("data-language") ?? "text" }), order),
            "blockquote" => Block(documentId, "quote", element.TextContent, null, order),
            "a" when element.ClassList.Contains("document-button") => Block(documentId, "button", element.TextContent, JsonSerializer.Serialize(new { url = element.GetAttribute("href"), target = element.GetAttribute("target") }), order),
            "hr" => Block(documentId, "separator", null, null, order),
            _ => Rich(documentId, element.OuterHtml, order)
        };
    }

    private static ProjectDocumentBlock Rich(uint documentId, string content, int order) => Block(documentId, "rich_text", content, null, order);
    private static ProjectDocumentBlock Block(uint documentId, string type, string? content, string? settings, int order) => new() { DocumentId = documentId, BlockType = type, Content = content, Settings = settings, DisplayOrder = order };

    private static HtmlSanitizer CreateSanitizer()
    {
        var value = new HtmlSanitizer();
        value.AllowedTags.UnionWith(["h1","h2","h3","h4","h5","h6","p","div","span","strong","b","em","i","u","s","strike","ul","ol","li","a","blockquote","table","thead","tbody","tfoot","tr","th","td","pre","code","hr","img","figure","figcaption","video","source","br"]);
        value.AllowedAttributes.UnionWith(["href","src","alt","title","target","rel","class","style","data-caption","data-language","data-media-id","controls"]);
        value.AllowedCssProperties.UnionWith(["color","background-color","font-size","font-family","font-weight","font-style","text-decoration","text-align","margin-left","padding-left"]);
        value.AllowedSchemes.Clear(); value.AllowedSchemes.UnionWith(["http","https"]);
        return value;
    }

    private static string? SafeVideoUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (Uri.TryCreate(value, UriKind.Relative, out _)) return value;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return null;
        return uri.Host is "youtube.com" or "www.youtube.com" or "youtube-nocookie.com" or "www.youtube-nocookie.com" or "youtu.be" or "vimeo.com" or "player.vimeo.com" ? value : null;
    }

    private static uint? GetMediaId(IElement element) => uint.TryParse(element.GetAttribute("data-media-id"), out var id) ? id : null;
}
