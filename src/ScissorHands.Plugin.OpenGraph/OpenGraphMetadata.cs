using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph;

/// <summary>
/// Resolves metadata once, independently of HTML serialization.
/// </summary>
internal sealed record OpenGraphMetadata(
    string? Title,
    string? Description,
    string? Locale,
    string Url,
    string ImageUrl,
    string? SiteName,
    string? TwitterSiteId,
    string? TwitterCreatorId)
{
    internal static OpenGraphMetadata Create(
        PluginManifest plugin,
        SiteManifest? site,
        ContentDocument? document,
        IEnumerable<ContentDocument>? documents = null)
    {
        // Resolve required context before deriving any output, including optional images.
        var url = OpenGraphPluginHelper.GetContentUrl(document, site);
        var imageUrl = OpenGraphPluginHelper.GetHeroImageUrl(document, site);
        var useContentMetadata = OpenGraphPluginHelper.UseContentMetadata(documents, document);
        var creatorId = OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterCreatorId");

        if (!string.IsNullOrWhiteSpace(document?.Metadata.TwitterHandle))
        {
            creatorId = document.Metadata.TwitterHandle;
        }

        if (!useContentMetadata || document?.Kind != ContentKind.Post)
        {
            creatorId = null;
        }

        return new OpenGraphMetadata(
            useContentMetadata ? $"{document!.Metadata.Title} | {site!.Title}" : site!.Title,
            useContentMetadata ? document!.Metadata.Description ?? site.Description : site.Description,
            site.Locale,
            url,
            imageUrl,
            site.Title,
            OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterSiteId"),
            creatorId);
    }
}
