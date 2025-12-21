using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics;

/// <summary>
/// This represents the plugin entity for Google Analytics.
/// </summary>
public class GoogleAnalyticsPlugin : ContentPlugin
{
    private const string GOOGLE_ANALYTICS_SCRIPT = """
    <!-- Google tag (gtag.js) -->
    <script async src="https://www.googletagmanager.com/gtag/js?id={{MEASUREMENT_ID}}"></script>
    <script>
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    gtag('config', '{{MEASUREMENT_ID}}');
    </script>
    """;

    private const string PLACEHOLDER = "<plugin:google-analytics></plugin:google-analytics>";

    /// <inheritdoc />
    public override string Name => "Google Analytics";

    /// <inheritdoc />
    public override async Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var measurementId = plugin.Options is null
            ? default
            : plugin.Options.TryGetValue("MeasurementId", out var measurementIdValue) && measurementIdValue is string measurementIdString
                ? measurementIdString
                : default;

        var script = GOOGLE_ANALYTICS_SCRIPT.Replace("{{MEASUREMENT_ID}}", measurementId, StringComparison.OrdinalIgnoreCase);

        html = html.Replace(PLACEHOLDER, $"\n{script}\n", StringComparison.OrdinalIgnoreCase);

        return await Task.FromResult(html);
    }
}
