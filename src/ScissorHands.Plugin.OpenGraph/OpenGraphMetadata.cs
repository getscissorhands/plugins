using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Urls;

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
        IEnumerable<ContentDocument>? documents = null,
        LocaleContext? localeContext = null,
        ThemeSettings? themeSettings = null)
    {
        // Resolve required context before deriving any output, including optional images.
        var url = OpenGraphPluginHelper.GetContentUrl(document, site, localeContext);
        var imageUrl = OpenGraphPluginHelper.GetHeroImageUrl(document, site, themeSettings);
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
            GetContentLocale(site, document, localeContext),
            url,
            imageUrl,
            site.Title,
            OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterSiteId"),
            creatorId);
    }

    private static string? GetContentLocale(SiteManifest site, ContentDocument? document, LocaleContext? localeContext)
    {
        if (!site.IsLocalizationEnabled)
        {
            return null;
        }

        // A fallback's requested locale belongs to its notice/UI, not its article.
        if (!string.IsNullOrWhiteSpace(localeContext?.ContentLocale))
        {
            return localeContext.ContentLocale;
        }

        return !string.IsNullOrWhiteSpace(document?.Metadata.Locale)
            ? document.Metadata.Locale
            : ContentUrlHelper.GetLocaleSegment(site.Locales[0]);
    }
}
