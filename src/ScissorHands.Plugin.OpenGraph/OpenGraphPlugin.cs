using System.Text;
using System.Text.Encodings.Web;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph;

/// <summary>
/// This represents the plugin entity for Open Graph.
/// </summary>
public sealed class OpenGraphPlugin : ContentPlugin
{
    private const string PLACEHOLDER = "<plugin:open-graph></plugin:open-graph>";

    /// <inheritdoc />
    public override string Id => "open-graph";

    /// <inheritdoc />
    public override string Name => "Open Graph";

    /// <inheritdoc />
    public override async Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = OpenGraphMetadata.Create(plugin, site, document);
        var output = new StringBuilder();

        AppendMeta(output, "property", "og:title", metadata.Title);
        AppendMeta(output, "property", "og:description", metadata.Description);
        AppendMeta(output, "property", "og:type", "website");
        AppendMeta(output, "property", "og:locale", metadata.Locale);
        AppendMeta(output, "property", "og:url", metadata.Url);
        if (!string.IsNullOrEmpty(metadata.ImageUrl))
        {
            AppendMeta(output, "property", "og:image", metadata.ImageUrl);
        }
        AppendMeta(output, "property", "og:site_name", metadata.SiteName);
        AppendMeta(output, "name", "twitter:card", "summary_large_image");
        if (!string.IsNullOrWhiteSpace(metadata.TwitterSiteId))
        {
            AppendMeta(output, "name", "twitter:site", metadata.TwitterSiteId);
        }
        if (!string.IsNullOrWhiteSpace(metadata.TwitterCreatorId))
        {
            AppendMeta(output, "name", "twitter:creator", metadata.TwitterCreatorId);
        }
        AppendMeta(output, "name", "twitter:title", metadata.Title);
        AppendMeta(output, "name", "twitter:description", metadata.Description);
        if (!string.IsNullOrEmpty(metadata.ImageUrl))
        {
            AppendMeta(output, "name", "twitter:image", metadata.ImageUrl);
        }

        // Replace only the original HTML's markers. Encoded metadata is never a template.
        return await Task.FromResult(html.Replace(PLACEHOLDER, output.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    private static void AppendMeta(StringBuilder output, string attribute, string key, string? value)
    {
        output.Append("<meta ").Append(attribute).Append("=\"").Append(key)
            .Append("\" content=\"").Append(HtmlEncoder.Default.Encode(value ?? string.Empty))
            .Append("\" />\n");
    }
}
