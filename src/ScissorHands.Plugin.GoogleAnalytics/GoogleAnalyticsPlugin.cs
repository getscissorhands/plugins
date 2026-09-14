using System.Text.Encodings.Web;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics;

/// <summary>
/// This represents the plugin entity for Google Analytics.
/// </summary>
public sealed class GoogleAnalyticsPlugin : ContentPlugin
{
    private const string GOOGLE_ANALYTICS_SCRIPT = """
    <!-- Google tag (gtag.js) -->
    <script async src="{{LOADER_URL}}"></script>
    <script>
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    gtag('config', '{{JAVASCRIPT_MEASUREMENT_ID}}');
    </script>
    """;

    private const string PLACEHOLDER = "<plugin:google-analytics></plugin:google-analytics>";

    /// <inheritdoc />
    public override string Id => "google-analytics";

    /// <inheritdoc />
    public override string Name => "Google Analytics";

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The configured MeasurementId is missing or invalid.</exception>
    /// <exception cref="OperationCanceledException">Cancellation is requested before processing.</exception>
    public override async Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var measurementId = GoogleAnalyticsConfiguration.GetMeasurementId(plugin.Options);
        var loaderUrl = HtmlEncoder.Default.Encode(GoogleAnalyticsConfiguration.GetLoaderUrl(measurementId));
        var javaScriptMeasurementId = GoogleAnalyticsConfiguration.GetJavaScriptMeasurementId(measurementId);
        var script = GOOGLE_ANALYTICS_SCRIPT
            .Replace("{{LOADER_URL}}", loaderUrl, StringComparison.Ordinal)
            .Replace("{{JAVASCRIPT_MEASUREMENT_ID}}", javaScriptMeasurementId, StringComparison.Ordinal);

        html = html.Replace(PLACEHOLDER, $"\n{script}\n", StringComparison.OrdinalIgnoreCase);

        return await Task.FromResult(html);
    }
}
