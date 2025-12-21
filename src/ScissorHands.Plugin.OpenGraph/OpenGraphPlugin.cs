using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph;

/// <summary>
/// This represents the plugin entity for Open Graph.
/// </summary>
public class OpenGraphPlugin : ContentPlugin
{
    private const string OPEN_GRAPH_TEMPLATE = """
    <meta property="og:title" content="{{CONTENT_TITLE}}" />
    <meta property="og:description" content="{{CONTENT_DESCRIPTION}}" />
    <meta property="og:type" content="website" />
    <meta property="og:locale" content="{{CONTENT_LOCALE}}" />
    <meta property="og:url" content="{{CONTENT_URL}}" />
    <meta property="og:image" content="{{CONTENT_HERO_IMAGE_URL}}" />
    <meta property="og:site_name" content="{{SITE_NAME}}" />

    <meta name="twitter:card" content="summary_large_image">
    {{TWITTER_CARD_SITE}}
    {{TWITTER_CARD_CREATOR}}
    <meta name="twitter:title" content="{{CONTENT_TITLE}}">
    <meta name="twitter:description" content="{{CONTENT_DESCRIPTION}}">
    <meta name="twitter:image" content="{{CONTENT_HERO_IMAGE_URL}}">
    """;

    private const string PLACEHOLDER = "<plugin:open-graph></plugin:open-graph>";

    /// <inheritdoc />
    public override string Name => "Open Graph";

    /// <inheritdoc />
    public override async Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? siteId;
        string? creatorId;

        if (plugin.Options is null)
        {
            siteId = default;
            creatorId = default;
        }
        else
        {
            siteId = plugin.Options.TryGetValue("TwitterSiteId", out var siteIdValue) && siteIdValue is string siteIdString
                            ? siteIdString : default;

            creatorId = plugin.Options.TryGetValue("TwitterCreatorId", out var creatorIdValue) && creatorIdValue is string creatorIdString
                            ? creatorIdString : default;
        }

        var template = OPEN_GRAPH_TEMPLATE;
        template = siteId is null
                    ? template.Replace("{{TWITTER_CARD_SITE}}", string.Empty, StringComparison.OrdinalIgnoreCase)
                    : template.Replace("{{TWITTER_CARD_SITE}}", $"<meta name=\"twitter:site\" content=\"{siteId}\">", StringComparison.OrdinalIgnoreCase);

        creatorId = document.Metadata.TwitterHandle is null ? creatorId : document.Metadata.TwitterHandle;
        creatorId = document.Kind == ContentKind.Post ? creatorId : default;

        template = creatorId is null
                    ? template.Replace("{{TWITTER_CARD_CREATOR}}", string.Empty, StringComparison.OrdinalIgnoreCase)
                    : template.Replace("{{TWITTER_CARD_CREATOR}}", $"<meta name=\"twitter:creator\" content=\"{creatorId}\">", StringComparison.OrdinalIgnoreCase);

        template = template.Replace("{{CONTENT_TITLE}}", document.Metadata.Title, StringComparison.OrdinalIgnoreCase);
        template = template.Replace("{{CONTENT_DESCRIPTION}}", document.Metadata.Description ?? site.Description, StringComparison.OrdinalIgnoreCase);
        template = template.Replace("{{CONTENT_LOCALE}}", site.Locale, StringComparison.OrdinalIgnoreCase);
        template = template.Replace("{{CONTENT_URL}}", $"{OpenGraphPluginHelper.GetContentUrl(document, site)}", StringComparison.OrdinalIgnoreCase);
        template = template.Replace("{{CONTENT_HERO_IMAGE_URL}}", $"{OpenGraphPluginHelper.GetHeroImageUrl(document, site)}", StringComparison.OrdinalIgnoreCase);
        template = template.Replace("{{SITE_NAME}}", site.Title, StringComparison.OrdinalIgnoreCase);

        html = html.Replace(PLACEHOLDER, $"{template}\n\n", StringComparison.OrdinalIgnoreCase);

        return await Task.FromResult(html);
    }
}
